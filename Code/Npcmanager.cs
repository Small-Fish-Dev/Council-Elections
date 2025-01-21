namespace Sandbox;

public sealed class Npcmanager : Component
{
	[Property]
	public List<GameObject> Voters { get; set; }

	[Property]
	public List<GameObject> LinePoints { get; set; }

	[Property]
	public GameObject NpcSpawn { get; set; }

	[Property]
	public GameObject NpcInterior { get; set; }

	[Property]
	public GameObject NpcDespawn { get; set; }

	public List<Actor> Actors { get; set; } = new();
	public static Npcmanager Instance { get; private set; }


	private TimeUntil _nextSpawn;

	protected override void OnStart()
	{
		Instance = this;

		if ( IsProxy ) return;

		foreach ( var linePoint in LinePoints )
		{
			var spawned = SpawnVoter( linePoint.WorldPosition );
			spawned.WorldRotation *= Rotation.FromYaw( -90f );
			if ( spawned.Components.TryGet<Actor>( out var actor ) )
				actor.WalkTo( NpcInterior.WorldPosition );
		}
	}

	internal TimeUntil _nextCheck;

	protected override void OnFixedUpdate()
	{
		if ( IsProxy ) return;

		if ( _nextSpawn )
		{
			if ( Voters.Count < 40 )
			{
				var spawned = SpawnVoter( NpcSpawn.WorldPosition );

				if ( spawned.Components.TryGet<Actor>( out var actor ) )
					actor.WalkTo( NpcInterior.WorldPosition );
			}

			_nextSpawn = 9f;
		}

		if ( _nextCheck )
		{
			foreach ( var actor in Actors )
			{
				if ( !actor.IsValid() ) continue;

				if ( actor.InLine )
				{
					var distanceFromLine = actor.WorldPosition.Distance( LinePoints.FirstOrDefault().WorldPosition );

					if ( distanceFromLine <= 20f )
					{
						actor.InLine = false;
						actor.Inside = true;
						actor.CanMove = Game.Random.Float( 3f, 8f );
					}
				}

				if ( actor.Inside )
				{
					var distanceFromInside = actor.WorldPosition.Distance( NpcInterior.WorldPosition );

					if ( distanceFromInside <= 20f )
					{
						actor.Inside = false;

						if ( actor is Voter voter )
						{
							actor.WalkTo( GetVotingPosition( voter.Pick ) );
							actor.Voting = true;
						}
						else
						{
							actor.WalkTo( NpcDespawn.WorldPosition );
							actor.Exiting = true;
						}
					}
				}

				if ( actor.Voting )
				{
					var distanceFromVote = actor.WorldPosition.Distance( GetVotingPosition( ((Voter)actor).Pick ) );

					if ( distanceFromVote <= 20f )
					{
						actor.Voting = false;
						actor.WalkTo( NpcDespawn.WorldPosition );
						actor.Exiting = true;
						actor.CanMove = Game.Random.Float( 1f );

						ElectionsManager.Instance.Voted( actor.WorldPosition );
					}
				}

				if ( actor.Exiting )
				{
					var distanceFromDespawn = actor.WorldPosition.Distance( NpcDespawn.WorldPosition );

					if ( distanceFromDespawn <= 50f )
						actor.DestroyGameObject();
				}
			}

			_nextCheck = 0.1f;
		}
	}

	public Vector3 GetVotingPosition( Candidate candidate )
	{
		var foundCandidate = ElectionsManager.Instance.Candidates.FirstOrDefault( x => x.CandidateId == candidate.CandidateId );
		var candidateObject = foundCandidate.SceneCandidate?.GameObject ?? null;

		if ( !candidateObject.IsValid() )
			return WorldPosition;

		return candidateObject.WorldPosition + candidateObject.WorldRotation.Forward * 100f;
	}

	public GameObject SpawnVoter( Vector3 position )
	{
		var toSpawn = Game.Random.FromList( Voters );
		var spawned = toSpawn.Clone( position );
		spawned.NetworkSpawn();

		if ( spawned.Components.TryGet<Actor>( out var actor ) )
			Actors.Add( actor );

		return spawned;
	}

	protected override void DrawGizmos()
	{
		using ( Gizmo.Scope( "world", global::Transform.Zero ) )
		{
			var draw = Gizmo.Draw;

			foreach ( var linePoint in LinePoints )
			{
				draw.Color = Color.Yellow.WithAlpha( 0.5f );
				draw.SolidSphere( linePoint.WorldPosition, 12f, 32, 32 );
				draw.Color = Color.White;
				var textPos = linePoint.WorldPosition + Vector3.Up * 5f;
				var textRot = linePoint.WorldRotation * Rotation.FromYaw( 270f ) * Rotation.FromRoll( 90f );
				draw.WorldText( LinePoints.IndexOf( linePoint ).ToString(), new Transform( textPos, textRot ) );
			}

			draw.Color = Color.Green.WithAlpha( 0.5f );
			draw.SolidSphere( NpcSpawn.WorldPosition, 20f, 32, 32 );

			draw.Color = Color.Blue.WithAlpha( 0.5f );
			draw.SolidSphere( NpcInterior.WorldPosition, 20f, 32, 32 );

			draw.Color = Color.Red.WithAlpha( 0.5f );
			draw.SolidSphere( NpcDespawn.WorldPosition, 20f, 32, 32 );

			draw.Color = Color.Magenta.WithAlpha( 0.5f );

			if ( ElectionsManager.Instance.IsValid() )
			{
				foreach ( var candidate in ElectionsManager.Instance.Candidates )
				{
					draw.SolidSphere( GetVotingPosition( candidate ), 20f, 32, 32 );
				}
			}
		}
	}


	[ConCmd( "kill_voters" )]
	public static void KillVoters()
	{
		if ( Connection.Local != Connection.Host ) return;

		foreach ( var voter in Instance.Actors )
			voter.DestroyGameObject();

		Log.Info( "All voters have been slain" );
	}
}
