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

	public IInteractable CurrentInteraction;

	protected override void OnStart()
	{
		ApplyClothing();

		Player.All.Add( this );

		if ( !IsProxy )
			Player.Local = this;
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
			if ( interactTrace.GameObject.Components.TryGet<IInteractable>( out var interaction ) )
				CurrentInteraction = interaction;
		}

		if ( Input.Pressed( "use" ) )
			if ( CurrentInteraction != null && CurrentInteraction.NextInteraction )
				CurrentInteraction.Interact();
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
}
