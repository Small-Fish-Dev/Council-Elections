
[Icon( "ballot" )]
public class Candidate
{
	/// <summary>
	/// Unique ID for the candidate, used for votes too
	/// </summary>
	[Property]
	public int CandidateId { get; set; }

	/// <summary>
	/// Name/Username of candidate
	/// </summary>
	[Property]
	public string CandidateName { get; set; }

	/// <summary>
	/// For pronouns
	/// </summary>
	[Property]
	public Gender CandidateGender { get; set; }

	/// <summary>
	/// Which party they belong to
	/// </summary>
	[Property]
	public string CandidateParty { get; set; }

	/// <summary>
	/// Steam id of the candidate, for in game interactions
	/// </summary>
	[Property]
	public ulong CandidateSteamId { get; set; }

	/// <summary>
	/// The candidate NPC
	/// </summary>
	[Property]
	public President SceneCandidate { get; set; }

	public struct Policy
	{
		/// <summary>
		/// The short name of this policy
		/// </summary>
		[Property]
		public string Name { get; set; }

		/// <summary>
		/// A description on what the policy is or does, no period needed at the end
		/// </summary>
		[Property]
		[WideMode]
		public string Info { get; set; }
	}

	/// <summary>
	/// The policies of this candidate
	/// </summary>
	[Property]
	[InlineEditor]
	[WideMode]
	public List<Policy> CandidatePolicies { get; set; }

	public Policy RandomPolicy() => Game.Random.FromList( CandidatePolicies );

	public override string ToString() => $"[{CandidateId}] {CandidateName}";
}
