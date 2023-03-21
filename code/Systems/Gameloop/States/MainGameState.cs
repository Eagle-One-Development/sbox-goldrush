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

	public override void OnPlayerKilled( Player player )
	{
		base.OnPlayerKilled( player );

		if ( player.CanRespawn() ) return;


	}
}
