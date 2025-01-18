using Sandbox;

public sealed class Player : Component
{
	public static List<Player> All { get; private set; } = new List<Player>();
	public static Player Local { get; private set; }

	[Property]
	[Category( "Components" )]
	public SkinnedModelRenderer SkinnedModelRenderer { get; set; }

	[Property]
	[Category( "Components" )]
	public PlayerController Controller { get; set; }

	[Property]
	[Category( "Components" )]
	public CameraComponent Camera { get; set; }

	[Property]
	[Category( "Stats" )]
	public float InteractRange { get; set; } = 120f;

	public Interaction CurrentInteraction;

	protected override void OnStart()
	{
		ApplyClothing();

		Player.All.Add( this );

		if ( !IsProxy )
			Player.Local = this;

		if ( IsProxy && Camera.IsValid() )
			Camera.DestroyGameObject();
	}

	protected override void OnDestroy()
	{
		Player.All.Remove( this );
	}

	protected override void OnFixedUpdate()
	{
		if ( !Camera.IsValid() ) return;

		CurrentInteraction = null;

		var interactTrace = Scene.Trace.Ray( Camera.WorldPosition, Camera.WorldPosition + Camera.WorldRotation.Forward * InteractRange )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();

		if ( interactTrace.Hit )
		{
			if ( interactTrace.GameObject.Components.TryGet<Interaction>( out var interaction ) )
				CurrentInteraction = interaction;
		}

		if ( Input.Pressed( "use" ) )
			if ( CurrentInteraction.IsValid() && CurrentInteraction.CanInteract )
				CurrentInteraction.Interact( this );
	}

	internal void ApplyClothing()
	{
		if ( Network.Owner == null )
			return;
		if ( !SkinnedModelRenderer.IsValid() )
			return;

		var container = ClothingContainer.CreateFromLocalUser();
		container.Apply( SkinnedModelRenderer );
	}

	/// <summary>
	/// Submit your vote through s&box stats service
	/// </summary>
	/// <param name="candidate"></param>
	public void Vote( Candidate candidate )
	{
		if ( IsProxy ) return;

		Sandbox.Services.Stats.SetValue( "vote", candidate.CandidateId );
		Log.Info( $"Voted for {candidate.CandidateName}" );
	}
}
