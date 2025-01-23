using System.Threading.Tasks;

namespace Sandbox;

public sealed class ElectionsNetworkHelper : Component, Component.INetworkListener
{
	[Property]
	public GameObject PlayerPrefab { get; set; }

	[Property]
	public List<GameObject> Spawners { get; set; }

	[Property]
	public GameObject ApeTavernSpawn { get; set; }

	protected override async Task OnLoad()
	{
		if ( Scene.IsEditor )
			return;

		if ( !Networking.IsActive )
		{
			LoadingScreen.Title = "Creating Lobby";
			await Task.DelayRealtimeSeconds( 0.1f );
			Networking.CreateLobby( new() );
		}
	}

	public void OnActive( Connection channel )
	{
		Log.Info( $"{channel.DisplayName} connected!" );
		var cloned = PlayerPrefab.Clone();

		if ( ApeTavern.IsApe( channel.SteamId ) )
			cloned.WorldPosition = ApeTavernSpawn.WorldPosition;
		else
			cloned.WorldPosition = Game.Random.FromList( Spawners ).WorldPosition;

		if ( cloned.Components.TryGet<Player>( out var player ) )
			player.SteamId = channel.SteamId;

		cloned.NetworkSpawn( channel );
	}

	/// <summary>
	/// Close a lobby
	/// </summary>
	[ConCmd( "lobby_close" )]
	public static void DebugCloseLobby()
	{
		if ( !Player.Local.IsPresident && !Connection.Local.IsHost ) return;

		CloseLobby();
	}

	[Rpc.Broadcast]
	public static void CloseLobby()
	{
		Networking.Disconnect();
		Game.Close();
	}
}
