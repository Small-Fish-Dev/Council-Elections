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
		if ( IsProxy ) return;

		Pick = ElectionsManager.RandomCandidate();
		DefaultExpression = (Expression)Game.Random.Int( 0, 6 );

		base.OnStart();
	}

	public override void Talk( GameObject target )
	{
		var randomMessage = ElectionsManager.CleanMessage( Game.Random.FromList( InteractPhrases ), Pick, out var isAboutOpponent );
		StartTalk( randomMessage, target );

		if ( isAboutOpponent )
			AngryFace();
		else
			HappyFace();
	}
}
