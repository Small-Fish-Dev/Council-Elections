using System;
using System.Text.Json;

namespace Sandbox;

public sealed class ElectionsManager : Component
{
	[Property]
	public List<Candidate> Candidates { get; set; } = new();
	[Property]
	public GameObject VotedParticle { get; set; }
	public Dictionary<int, int> CurrentResults { get; set; } = new();

	[Sync( SyncFlags.FromHost )]
	public TimeUntil NextResults { get; set; } = 60f;

	public static ElectionsManager Instance { get; private set; }

	protected override void OnAwake()
	{
		Instance = this;
	}

	protected override void OnStart()
	{
		QueryResults();
	}

	protected override void OnFixedUpdate()
	{
		if ( NextResults )
		{
			QueryResults();
			NextResults = 60f; // 1 minute
		}
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
	/// <param name="isAboutOpponent"></param>
	/// <returns></returns>
	public static string CleanMessage( string message, Candidate pick, out bool isAboutOpponent )
	{
		isAboutOpponent = false;
		if ( string.IsNullOrWhiteSpace( message ) ) return message; // Don't bother..

		Candidate target = ElectionsManager.Instance.Candidates.FirstOrDefault( x => x.CandidateId == pick.CandidateId );

		if ( message.Contains( "<pick>", StringComparison.OrdinalIgnoreCase ) )
		{
			message = message.Replace( "<pick>", "", StringComparison.OrdinalIgnoreCase );
			target = ElectionsManager.Instance.Candidates.FirstOrDefault( x => x.CandidateId == pick.CandidateId );
			isAboutOpponent = false;
		}

		if ( message.Contains( "<opponent>", StringComparison.OrdinalIgnoreCase ) )
		{
			message = message.Replace( "<opponent>", "", StringComparison.OrdinalIgnoreCase );
			target = ElectionsManager.RandomCandidate( pick.CandidateId );
			isAboutOpponent = true;
		}

		// GENDER //
		var gender = target.CandidateGender;
		ReplacePronoun( ref message, "they're", gender, "he's", "she's", "they're" );
		ReplacePronoun( ref message, "they are", gender, "he is", "she is", "they are" );
		ReplacePronoun( ref message, "are they", gender, "is he", "is she", "are they" );
		ReplacePronoun( ref message, "they", gender, "he", "she", "they" ); // Do this after "they are" and "they're" or else it ruins those
		ReplacePronoun( ref message, "theirs", gender, "his", "hers", "theirs" );
		ReplacePronoun( ref message, "their", gender, "his", "her", "their" );
		ReplacePronoun( ref message, "themself", gender, "himself", "herself", "themself" );
		ReplacePronoun( ref message, "them", gender, "him", "her", "them" );

		// CANDIDATE //

		ReplaceWord( ref message, "<name>", target.CandidateName );
		var randomPolicy = target.RandomPolicy();
		ReplaceWord( ref message, "<policy.name>", randomPolicy.Name );
		ReplaceWord( ref message, "<policy.info>", randomPolicy.Info );

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
		if ( string.IsNullOrWhiteSpace( keyword ) || string.IsNullOrWhiteSpace( replacement ) ) return;

		if ( message.Contains( keyword, StringComparison.OrdinalIgnoreCase ) )
		{
			message = message.Replace( keyword, replacement );
			var capitalKeyword = char.ToUpper( keyword[0] ) + keyword.Substring( 1 );
			var capitalPronoun = char.ToUpper( replacement[0] ) + replacement.Substring( 1 );
			message = message.Replace( capitalKeyword, capitalPronoun );
		}
	}

	[Rpc.Broadcast]
	public void Voted( Vector3 position )
	{
		VotedParticle.Clone( position );
		Sound.Play( "gift", position );
	}

	public async void QueryResults()
	{
		var results = Sandbox.Services.Leaderboards.Get( "vote" );
		results.MaxEntries = 999;
		await results.Refresh();
		var oldResults = CurrentResults.Clone();

		CurrentResults.Clear();

		foreach ( var candidate in Candidates )
			CurrentResults.Add( candidate.CandidateId, 0 );

		foreach ( var entry in results.Entries )
		{
			var candidate = (int)entry.Value;

			if ( CurrentResults.ContainsKey( candidate ) ) // No write down candidates!
				CurrentResults[candidate]++;
		}

		Log.Info( "Results updated." );

		foreach ( var candidate in Candidates )
		{
			if ( CurrentResults.TryGetValue( candidate.CandidateId, out var votes ) )
				Log.Info( $"{candidate.CandidateName}: {votes} votes." );
			else
				Log.Info( $"{candidate.CandidateName}: 0 votes." );
		}

		if ( oldResults.Count() < CurrentResults.Count() || CurrentResults.Any( x => oldResults[x.Key] != x.Value ) )
		{
			GenerateChart( CurrentResults );
			Log.Info( "Generating updated chart..." );
		}
		else
			Log.Info( "No new chart to generate" );
	}

	public Color[] Colors { get; set; } = new Color[7]
	{
		new Color(127 / 255f, 255 / 255f, 0 / 255f),
		new Color(205 / 255f, 92 / 255f, 92 / 255f),
		new Color(255 / 255f, 215 / 255f, 0 / 255f),
		new Color(255 / 255f, 140 / 255f, 0 / 255f),
		new Color(30 / 255f, 144 / 255f, 255 / 255f),
		new Color(0 / 255f, 255 / 255f, 200 / 255f),
		new Color(255 / 255f, 50 / 255f, 255 / 255f)
	};

	public Texture CurrentResultsImage { get; set; }

	public void GenerateChart( Dictionary<int, int> results )
	{
		var entries = new ChartEntry[Candidates.Count];

		foreach ( var candidate in Candidates )
		{
			var votes = results.FirstOrDefault( x => x.Key == candidate.CandidateId );
			entries[candidate.CandidateId] = new ChartEntry( candidate.CandidateName, Colors[candidate.CandidateId], votes.Value );
		}

		var chart = JsonSerializer.Serialize( new Chart( entries ) );
		CurrentResultsImage = QuickChartApi.GetChartImage( chart );
	}

	[ConCmd( "reset_cooldowns" )]
	public static void ResetCooldowns()
	{
		if ( Connection.Local != Connection.Host ) return;

		foreach ( var interaction in Game.ActiveScene.GetAllComponents<Interaction>() )
			interaction.NextInteraction = 0f;

		Log.Info( "All cooldowns have been reset" );
	}
}
