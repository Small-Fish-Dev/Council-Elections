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

	public override void Talk( Player target )
	{
		base.Talk( target );

		var randomMessage = ElectionsManager.CleanMessage( Game.Random.FromList( InteractPhrases ), Pick );
		var speechSpeed = 30f;
		var speechDuration = 2f;
		SpeechUI.AddSpeech( FullName, randomMessage, speechSpeed, speechDuration );

		TalkingTo = target;
		var duration = randomMessage.Count() / speechSpeed + speechDuration;
		StopTalking = duration;
		Interaction.InteractionCooldown = duration;
	}
}
