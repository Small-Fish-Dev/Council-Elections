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

	public override void Talk( GameObject target )
	{
		var randomMessage = ElectionsManager.CleanMessage( Game.Random.FromList( InteractPhrases ), Self, out var isAboutOpponent );
		StartTalk( randomMessage, target );

		if ( isAboutOpponent )
			AngryFace();
		else
			HappyFace();
	}
}
