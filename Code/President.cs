using Sandbox;
using Sandbox.Citizen;
using Sandbox.UI;
using System.Xml.Linq;

public partial class President : Actor
{
	[Property]
	public Candidate Self { get; private set; }

	[Property]

	public Material SkinMaterial { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
	}

	public override void Clothe()
	{
		foreach ( var clothing in ModelRenderer.GameObject.Children )
			if ( clothing.Components.TryGet<SkinnedModelRenderer>( out var renderer ) )
				renderer.SetMaterialOverride( SkinMaterial, "skin" );

		ModelRenderer.SetMaterialOverride( SkinMaterial, "skin" );
	}

	public override void Talk( Player target )
	{
		base.Talk( target );

		var randomMessage = ElectionsManager.CleanMessage( Game.Random.FromList( InteractPhrases ), Self, out var isAboutOpponent );
		var speechSpeed = 30f;
		var waitDuration = 2f;

		if ( !IsProxy )
			SpeechUI.AddSpeech( FullName, randomMessage, speechSpeed, waitDuration, GameObject, Gender, Pitch );

		LookingTo = target;
		var talkDuration = randomMessage.Count() / speechSpeed;
		var duration = talkDuration + waitDuration;
		StopTalking = talkDuration;
		StopLooking = duration;
		Interaction.InteractionCooldown = duration;

		if ( isAboutOpponent )
			AngryFace();
		else
			HappyFace();
	}

	public override void StopLook()
	{
		base.StopLook();
		NeutralFace();
	}
}
