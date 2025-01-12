namespace Sandbox;

public sealed class ElectionsManager : Component
{
	[Property]
	public List<Candidate> Candidates { get; set; } = new();
}
