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
	/// <param name="except">Doesn't pick this one, set -1 to pick any</param>
	/// <returns></returns>
	public static Candidate RandomCandidate( int except = -1 )
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
		if ( string.IsNullOrWhiteSpace( message ) ) return message; // Don't bother..

		// GENDER //
		var gender = pick.CandidateGender;
		ReplacePronoun( ref message, "they're", gender, "he's", "she's", "they're" );
		ReplacePronoun( ref message, "they are", gender, "he is", "she is", "they are" );
		ReplacePronoun( ref message, "they", gender, "he", "she", "they" ); // Do this after "they are" and "they're" or else it ruins those
		ReplacePronoun( ref message, "theirs", gender, "his", "hers", "theirs" );
		ReplacePronoun( ref message, "their", gender, "his", "her", "their" );
		ReplacePronoun( ref message, "themself", gender, "himself", "herself", "themself" );
		ReplacePronoun( ref message, "them", gender, "him", "her", "them" );

		// PICK POLICY //
		var randomPolicy = pick.RandomPolicy();
		ReplaceWord( ref message, "pick.policy.name", randomPolicy.Name );
		ReplaceWord( ref message, "pick.policy.info", randomPolicy.Info );

		// PICK //
		ReplaceWord( ref message, "pick", pick.CandidateName ); // We do this after pick.policy or else it would replace the "pick" in there too

		// OPPONENT POLICY //
		var randomOpponent = ElectionsManager.RandomCandidate( pick.CandidateId );
		var opponentPolicy = randomOpponent.RandomPolicy();
		ReplaceWord( ref message, "opponent.policy.name", opponentPolicy.Name );
		ReplaceWord( ref message, "opponent.policy.info", opponentPolicy.Info );

		// OPPONENT //
		ReplaceWord( ref message, "opponent", randomOpponent.CandidateName );

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

			ReplaceWord( ref message, keyword, pronoun );
		}
	}

	internal static void ReplaceWord( ref string message, string keyword, string replacement )
	{
		if ( message.Contains( keyword, StringComparison.OrdinalIgnoreCase ) )
		{
			message = message.Replace( keyword, replacement );
			var capitalKeyword = char.ToUpper( keyword[0] ) + keyword.Substring( 1 );
			var capitalPronoun = char.ToUpper( replacement[0] ) + replacement.Substring( 1 );
			message = message.Replace( capitalKeyword, capitalPronoun );
		}
	}
}
