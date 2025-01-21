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

	protected override void OnFixedUpdate()
	{
		if ( _nextSpawn )
		{
			var toSpawn = Game.Random.FromList( Voters );
			var spawned = toSpawn.Clone( NpcSpawn.WorldPosition );

			if ( spawned.Components.TryGet<Actor>( out var actor ) )
				actor.WalkTo( NpcInterior.WorldPosition );

			_nextSpawn = 0.3f;
		}
	}


	protected override void OnUpdate()
	{

	}
}
