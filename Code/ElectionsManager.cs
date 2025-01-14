namespace Sandbox;

public sealed class ElectionsManager : Component
{
	[Property]
	public List<Candidate> Candidates { get; set; } = new();

	public static ElectionsManager Instance { get; private set; }

	protected override void OnAwake()
	{
		Instance = this;
	}

	/// <summary>
	/// Picks a random candidate from the list
	/// </summary>
	/// <param name="except">Doesn't pick this one, set 0 to pick any</param>
	/// <returns></returns>
	public static Candidate RandomCandidate( int except = 0 )
	{
		var candidatesToPick = Instance.Candidates.Where( x => x.CandidateId != except ).ToList();
		return Game.Random.FromList( candidatesToPick );
	}
}
