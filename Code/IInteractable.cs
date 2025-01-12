public interface IInteractable
{
	public string InteractionName { get; set; }
	public string InteractionDescription { get; set; }
	public float InteractionCooldown { get; set; }
	public TimeUntil NextInteraction { get; set; }
	public Vector3 InteractionBounds { get; set; }

	public void Interact()
	{

	}
}
