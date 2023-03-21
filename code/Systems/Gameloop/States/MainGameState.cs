using GoldRush.Nexus;

namespace GoldRush;

[Library( "maingame" )]
public class MainGameState : GameState
{
	public override void OnClientJoined( IClient client )
	{
		base.OnClientJoined( client );

		if ( client.Pawn is not Player player )
			return;

		// respawn players at start of new round and give them weapons
		player.Respawn();
		player.GiveLoadout();
	}

	public override void OnStart()
	{
		base.OnStart();

		NexusEntity.ResetAll();
	}
}
