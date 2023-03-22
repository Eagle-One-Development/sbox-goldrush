using GoldRush.Nexus;

namespace GoldRush;

[Library( "maingame" )]
public class MainGameState : GameState
{
	public override void OnClientJoined( IClient client )
	{
		base.OnClientJoined( client );

		if ( client.Pawn is not Player player ) return;

		if ( IsActive )
			player.RespawnAsync( 3 );
	}

	public override void OnStart()
	{
		base.OnStart();

		foreach ( var client in Game.Clients )
		{
			if ( client.Pawn is not Player player ) continue;
			player.Respawn();
			player.GiveLoadout();
		}

		NexusEntity.ResetAll();
	}

	public override void OnPlayerKilled( Player victim )
	{
		base.OnPlayerKilled( victim );

		TryWin();
	}

	[Event( "nexus.destroy" )]
	private void TryWin()
	{
		Log.Info( $"check for win" );

		var teamHash = new HashSet<string>();

		// victim that cannot respawn just died. could be win condition

		foreach ( var client in Game.Clients )
		{
			if ( client.Pawn is not Player player ) continue;

			Log.Info( $"check player {player} RSPWN:{player.CanRespawn()} ALIVE:{player.IsAlive}" );

			if ( player.CanRespawn() || player.IsAlive )
				teamHash.Add( player.Team.Resource.Id );
		}

		Log.Info( $"team count {teamHash.Count}" );
		if ( teamHash.Count > 1 ) return;

		GameLoop.FinishWithWin( teamHash.First() );
	}
}
