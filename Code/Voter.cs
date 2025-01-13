using Sandbox;
using Sandbox.Citizen;
using Sandbox.UI;
using System.Xml.Linq;

public partial class Voter : Actor
{
	public Interaction Interaction { get; private set; }
	protected override void OnStart()
	{
		base.OnStart();

		if ( Components.TryGet<Interaction>( out var interaction, FindMode.EnabledInSelfAndDescendants ) )
		{
			Interaction = interaction;
			Interaction.InteractionName = FullName;
		}
	}
}
