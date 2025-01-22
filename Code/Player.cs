using Sandbox;
using System;
using System.Numerics;

public sealed class Player : Actor
{
	public static List<Player> All { get; private set; } = new List<Player>();
	public static Player Local { get; private set; }

	[Property]
	[Category( "Components" )]
	public PlayerController Controller { get; set; }

	[Property]
	[Category( "Components" )]
	public CameraComponent Camera { get; set; }

	[Property]
	[Category( "Stats" )]
	public float InteractRange { get; set; } = 120f;

	[Property]
	public GameObject NameTag { get; set; }

	[Sync]
	[Property]
	public bool HasVoted { get; set; } = false;

	[Sync( SyncFlags.FromHost )]
	[Property]
	public bool IsPresident { get; set; } = false;

	[Sync]
	public bool GunOut { get; set; } = false;

	[Property]
	public GameObject GunViewModel { get; set; }
	[Property]
	public GameObject GunWorldModel { get; set; }

	public Interaction CurrentInteraction;
	public string LastMessage { get; set; }
	ulong _steamId;

	protected override void OnStart()
	{
		Player.All.Add( this );

		if ( ElectionsManager.Instance.IsValid() && ElectionsManager.Instance.Candidates != null )
		{
			var presidentCopy = ElectionsManager.Instance.Candidates.FirstOrDefault( x => x.CandidateSteamId == Network.Owner.SteamId );

			if ( presidentCopy.CandidateId != 0 && presidentCopy.SceneCandidate.IsValid() )
			{
				presidentCopy.SceneCandidate.GameObject.Enabled = false;
				WorldPosition = presidentCopy.SceneCandidate.WorldPosition;
				IsPresident = true;
			}
		}

		_steamId = Network.Owner.SteamId;

		if ( !IsProxy )
		{
			Player.Local = this;

			if ( NameTag.IsValid() )
				NameTag.Enabled = false;

			var vote = Sandbox.Services.Stats.GetPlayerStats( Game.Ident, Connection.Local.SteamId )
				.FirstOrDefault( x => x.Name == "vote" );
			HasVoted = !vote.Equals( default( Sandbox.Services.Stats.PlayerStat ) );

			if ( HasVoted )
			{
				Sandbox.Services.Achievements.Unlock( "voted" );
				Tags.Add( "voted" );
				Log.Info( "Player has already voted, disabling ballots." );

				foreach ( var voteInteraction in Scene.GetAllComponents<Interaction>() )
					if ( !voteInteraction.SharedInteraction )
						voteInteraction.NextInteraction = 999f;
			}
			else
			{
				// If the owner has already voted and someone new joins, they get the snapshot from the owner so we reenable them
				foreach ( var voteInteraction in Scene.GetAllComponents<Interaction>() )
					if ( !voteInteraction.SharedInteraction )
						voteInteraction.NextInteraction = 0f;
			}

			var json = ClothingContainer
				.CreateFromLocalUser()
				.Serialize();

			BroadcastClothing( json );
		}
		else
		{
			if ( NameTag.IsValid() )
				NameTag.Enabled = true;
		}

		if ( IsProxy && Camera.IsValid() )
			Camera.Destroy();

		UserName = Network.Owner.DisplayName;
		Pitch = (Network.Owner.SteamId % 100) / 100f + 0.5f;
	}

	protected override void OnDestroy()
	{
		Player.All.Remove( this );

		var presidentCopy = ElectionsManager.Instance.Candidates.FirstOrDefault( x => x.CandidateSteamId == _steamId );

		if ( presidentCopy.CandidateId != 0 )
			if ( presidentCopy.SceneCandidate.IsValid() )
				presidentCopy.SceneCandidate.GameObject.Enabled = true;
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( !Camera.IsValid() ) return;
		if ( IsProxy ) return;

		if ( CurrentInteraction.IsValid() )
		{
			CurrentInteraction.HighlightOutline.Enabled = false;
			CurrentInteraction = null;
		}

		var interactTrace = Scene.Trace.Ray( Camera.WorldPosition, Camera.WorldPosition + Camera.WorldRotation.Forward * InteractRange )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();

		if ( interactTrace.Hit )
		{
			if ( interactTrace.GameObject.Components.TryGet<Interaction>( out var interaction ) )
			{
				CurrentInteraction = interaction;
				CurrentInteraction.HighlightOutline.Enabled = true;
			}
		}

		if ( Input.Pressed( "use" ) )
			if ( CurrentInteraction.IsValid() && CurrentInteraction.CanInteract )
				CurrentInteraction.Interact( this );

		if ( Input.Pressed( "gun" ) && IsPresident )
		{
			if ( GunOut )
				DisableGun();
			else
				EnableGun();
		}

	}
	public override void Talk( GameObject target )
	{
		StartTalk( LastMessage, null );
		HappyFace();
	}

	[Rpc.Broadcast]
	public void BroadcastClothing( string clothing )
	{
		if ( !ModelRenderer.IsValid() )
			return;

		var container = ClothingContainer.CreateFromJson( clothing );
		container.Apply( ModelRenderer );
	}

	/// <summary>
	/// Submit your vote through sbox stats service
	/// </summary>
	/// <param name="candidate"></param>
	public void Vote( Candidate candidate )
	{
		if ( IsProxy ) return;

		Sandbox.Services.Stats.SetValue( "vote", candidate.CandidateId );
		Sandbox.Services.Achievements.Unlock( "voted" );
		Log.Info( $"Voted for {candidate.CandidateName}" );
		Log.Info( "Disabling ballots." );

		foreach ( var voteInteraction in Scene.GetAllComponents<Interaction>() )
			if ( !voteInteraction.SharedInteraction )
				voteInteraction.NextInteraction = 999f;

		ElectionsManager.Instance.Voted( WorldPosition );
	}

	[Rpc.Broadcast]
	public void EnableGun()
	{
		GunOut = true;

		if ( GunWorldModel.IsValid() )
			GunWorldModel.Enabled = true;

		if ( GunViewModel.IsValid() )
			GunViewModel.Enabled = true;

		if ( ModelRenderer.IsValid() )
			ModelRenderer.Set( "holdtype", 1 );
	}

	[Rpc.Broadcast]
	public void DisableGun()
	{
		GunOut = false;

		if ( GunWorldModel.IsValid() )
			GunWorldModel.Enabled = false;

		if ( GunViewModel.IsValid() )
			GunViewModel.Enabled = false;

		if ( ModelRenderer.IsValid() )
			ModelRenderer.Set( "holdtype", 0 );
	}
}
