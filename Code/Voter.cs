using Sandbox;
using Sandbox.Citizen;
using Sandbox.UI;
using System.Xml.Linq;

public partial class Voter : Actor, IInteractable
{
	public string InteractionName { get; set; } = "John Doe";
	public string InteractionDescription { get; set; } = "Talk";
	public float InteractionCooldown { get; set; } = 2f;
	public TimeUntil NextInteraction { get; set; }
	public Vector3 InteractionBounds { get; set; }

	protected override void OnStart()
	{
		InteractionBounds = Collider.Scale;
	}

	public void Interact()
	{
		if ( NextInteraction )
		{
			Log.Info( "Fuck off!" );
			NextInteraction = InteractionCooldown;
		}
	}
}
