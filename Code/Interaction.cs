using System;

public sealed class Interaction : Component
{
	[Property]
	public string InteractionName { get; set; }

	[Property]
	public string InteractionDescription { get; set; }

	[Property]
	[Range( 0f, 5f, 0.1f )]
	public float InteractionCooldown { get; set; } = 1f;

	[Property]
	public Action<Player> PlayerAction { get; set; }

	public HighlightOutline HighlightOutline { get; private set; }

	public bool CanInteract => _nextInteraction;
	private TimeUntil _nextInteraction;

	protected override void OnStart()
	{
		HighlightOutline = GameObject.AddComponent<HighlightOutline>();
		HighlightOutline.Enabled = false;
	}

	/// <summary>
	/// Invoke the interaction, if cooldown is up
	/// </summary>
	/// <param name="player"></param>
	[Rpc.Broadcast]
	public void Interact( Player player )
	{
		if ( CanInteract )
		{
			PlayerAction?.Invoke( player );
			_nextInteraction = InteractionCooldown;
		}
	}
}
