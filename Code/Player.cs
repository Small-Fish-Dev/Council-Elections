using Sandbox;

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

	[Sync]
	public bool HasVoted { get; set; } = false;
	public Interaction CurrentInteraction;
	ulong _steamId;

	protected override void OnStart()
	{
		Player.All.Add( this );

		var presidentCopy = ElectionsManager.Instance.Candidates.FirstOrDefault( x => x.CandidateSteamId == Network.Owner.SteamId );

		if ( presidentCopy.CandidateId != 0 )
			if ( presidentCopy.SceneCandidate.IsValid() )
				presidentCopy.SceneCandidate.GameObject.Enabled = false;

		_steamId = Network.Owner.SteamId;

		if ( !IsProxy )
		{
			Player.Local = this;
			var vote = Sandbox.Services.Stats.GetPlayerStats( Game.Ident, Connection.Local.SteamId )
				.FirstOrDefault( x => x.Name == "vote" );
			HasVoted = !vote.Equals( default( Sandbox.Services.Stats.PlayerStat ) );

			if ( HasVoted )
			{
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

		if ( IsProxy && Camera.IsValid() )
			Camera.DestroyGameObject();

		UserName = Network.Owner.DisplayName;
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
		Log.Info( $"Voted for {candidate.CandidateName}" );
		Log.Info( "Disabling ballots." );

		foreach ( var voteInteraction in Scene.GetAllComponents<Interaction>() )
			if ( !voteInteraction.SharedInteraction )
				voteInteraction.NextInteraction = 999f;
	}
}
