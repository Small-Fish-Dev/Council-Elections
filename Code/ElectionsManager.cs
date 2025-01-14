using System;

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

	/// <summary>
	/// Replace placeholders in a text with the appropriate words
	/// </summary>
	/// <param name="message"></param>
	/// <param name="pick"></param>
	/// <returns></returns>
	public static string CleanMessage( string message, Candidate pick )
	{
		var gender = pick.CandidateGender;

		ReplacePronoun( ref message, "they", gender, "he", "she", "they" );
		ReplacePronoun( ref message, "they're", gender, "he's", "she's", "they're" );
		ReplacePronoun( ref message, "they are", gender, "he is", "she is", "they are" );
		ReplacePronoun( ref message, "their", gender, "his", "hers", "theirs" );
		ReplacePronoun( ref message, "them", gender, "him", "her", "them" );
		ReplacePronoun( ref message, "themself", gender, "himself", "herself", "themself" );

		return message;
	}

	internal static void ReplacePronoun( ref string message, string keyword, Gender gender, string malePronoun, string femalePronoun, string neutralPronoun )
	{
		if ( message.Contains( keyword, StringComparison.OrdinalIgnoreCase ) )
		{
			var pronoun = gender switch
			{
				Gender.Male => malePronoun,
				Gender.Female => femalePronoun,
				_ => neutralPronoun
			};

			message = message.Replace( keyword, pronoun );

			var capitalKeyword = char.ToUpper( keyword[0] ) + keyword.Substring( 1 );
			var capitalPronoun = char.ToUpper( pronoun[0] ) + pronoun.Substring( 1 );
			message = message.Replace( capitalKeyword, capitalPronoun );
		}
	}
}
