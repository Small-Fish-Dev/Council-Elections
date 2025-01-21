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


	private TimeUntil _nextSpawn;

	protected override void OnStart()
	{
		foreach ( var linePoint in LinePoints )
		{
			var spawned = SpawnVoter( linePoint.WorldPosition );
			spawned.WorldRotation *= Rotation.FromYaw( -90f );
			if ( spawned.Components.TryGet<Actor>( out var actor ) )
				actor.WalkTo( NpcInterior.WorldPosition );
		}
	}

	protected override void OnFixedUpdate()
	{
		if ( _nextSpawn )
		{
			var spawned = SpawnVoter( NpcSpawn.WorldPosition );

			if ( spawned.Components.TryGet<Actor>( out var actor ) )
				actor.WalkTo( NpcInterior.WorldPosition );

			_nextSpawn = 2f;
		}
	}

	public GameObject SpawnVoter( Vector3 position )
	{
		var toSpawn = Game.Random.FromList( Voters );
		var spawned = toSpawn.Clone( position );
		spawned.NetworkSpawn();

		if ( spawned.Components.TryGet<Actor>( out var actor ) )
			actor.WalkTo( position );

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
		}
	}
}
