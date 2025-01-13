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

	public bool CanInteract => _nextInteraction;
	private TimeUntil _nextInteraction;

	/// <summary>
	/// Invoke the interaction, if cooldown is up
	/// </summary>
	/// <param name="player"></param>
	public void Interact( Player player )
	{
		if ( CanInteract )
		{
			_nextInteraction = InteractionCooldown;
			PlayerAction?.Invoke( player );
		}
	}
}
