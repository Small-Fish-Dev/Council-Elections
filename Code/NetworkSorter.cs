using Sandbox.Network;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Sandbox.PhysicsGroupDescription;

namespace Sandbox;

public sealed class NetworkSorter : Component
{
	[Property]
	public SceneFile SceneFile { get; set; }

	protected override async Task OnLoad()
	{
		LoadingScreen.Title = "Searching lobbies...";
		await Task.DelayRealtimeSeconds( 0.1f );
		try
		{
			var lobbies = await Networking.QueryLobbies();

			if ( lobbies == null || lobbies.Count() == 0 ) // No lobbies or invalid
			{
				LoadingScreen.Title = "No lobbies found, creating one...";
				Log.Info( "No lobbies found, creating one..." );
				CreateAndLoad();
			}
			else
			{
				if ( lobbies.All( x => x.IsFull ) )
				{
					LoadingScreen.Title = "All lobbies full, creating one...";
					Log.Info( "All lobbies full, creating one..." );
					CreateAndLoad();
				}
				else
				{
					LoadingScreen.Title = "Lobby found, joining...";
					Log.Info( "Lobby found, joining..." );
					var joined = await Networking.JoinBestLobby( Game.Ident );

					if ( !joined )
					{
						LoadingScreen.Title = "Could not join, trying newest lobby...";
						Log.Info( "Could not join, trying newest lobby..." );

						var freeLobbies = lobbies.Where( x => !x.IsFull );
						var newestLobby = freeLobbies.OrderByDescending( x => long.Parse( x.Name ) )
							.FirstOrDefault(); // Join the latest lobby opened, in case there's toxic ones

						Networking.Connect( newestLobby.LobbyId );
					}
				}
			}

			await Task.DelaySeconds( 10f );

			if ( Connection.Local.IsConnecting )
			{
				LoadingScreen.Title = "Creating lobby...";
				Log.Info( $"Couldn't connect, creating lobby..." );
				CreateAndLoad();
			}
		}
		catch ( Exception exception )
		{
			LoadingScreen.Title = "Creating lobby...";
			Log.Info( $"{exception} when joining, creating lobby..." );
			CreateAndLoad();
		}
	}

	internal void CreateAndLoad()
	{
		var currentTime = DateTime.UtcNow.Ticks;

		Networking.CreateLobby( new LobbyConfig()
		{
			MaxPlayers = 32,
			Privacy = LobbyPrivacy.Public,
			Name = currentTime.ToString(),
			DestroyWhenHostLeaves = false,
			AutoSwitchToBestHost = true,
			Hidden = false
		} );

		if ( Scene.IsValid() && SceneFile.IsValid() )
			Scene.Load( SceneFile );
		else
			Game.Close(); // Give up lil bro
	}
}
