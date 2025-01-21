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


	protected override void OnUpdate()
	{

	}
}
