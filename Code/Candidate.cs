
[Icon( "ballot" )]
public struct Candidate
{
	[Property]
	public int CandidateId { get; set; }

	[Property]
	public string CandidateName { get; set; }

	[Property]
	public GameObject CandidatePrefab { get; set; }

	public override string ToString() => $"[{CandidateId}] {CandidateName}";
}
