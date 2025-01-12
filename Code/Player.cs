using Sandbox;

public sealed class Player : Component
{
	public static List<Player> All { get; private set; } = new List<Player>();
	public SkinnedModelRenderer SkinnedModelRenderer { get; private set; }

	protected override void OnStart()
	{
		if ( Components.TryGet<SkinnedModelRenderer>( out var renderer, FindMode.EverythingInSelfAndDescendants ) )
			SkinnedModelRenderer = renderer;

		ApplyClothing();

		Player.All.Add( this );
	}

	protected override void OnDestroy()
	{
		Player.All.Remove( this );
	}

	protected override void OnUpdate()
	{

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
