using Sandbox;
using Sandbox.Citizen;
using Sandbox.UI;
using System.Xml.Linq;

public partial class Voter : Actor
{
	/// <summary>
	///  Who this voter is going to vote
	/// </summary>
	public Candidate Pick { get; private set; }

	protected override void OnStart()
	{
		base.OnStart();

		Pick = ElectionsManager.RandomCandidate();
	}

	public void Talk()
	{
		Log.Info( Game.Random.FromList( InteractPhrases ) );
	}
}
