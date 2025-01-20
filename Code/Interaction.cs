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

	/// <summary>
	/// Does the cooldown reset on other clients too
	/// </summary>
	[Property]
	public bool SharedInteraction { get; set; } = true;

	[Property]
	public Action<Player> PlayerAction { get; set; }

	public HighlightOutline HighlightOutline { get; private set; }

	public bool CanInteract => NextInteraction;
	public TimeUntil NextInteraction;

	protected override void OnStart()
	{
		HighlightOutline?.Destroy();
		HighlightOutline = GameObject.AddComponent<HighlightOutline>();
		HighlightOutline.Enabled = false;
	}

	/// <summary>
	/// Invoke the interaction, if cooldown is up
	/// </summary>
	/// <param name="player"></param>
	public void Interact( Player player )
	{
		if ( !SharedInteraction && player.IsProxy ) return;

		if ( CanInteract )
		{
			PlayerAction?.Invoke( player );
			player.HasVoted = true;
			NextInteraction = InteractionCooldown;
		}
	}
}
