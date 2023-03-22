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
		var teamHash = new HashSet<string>();

		foreach ( var client in Game.Clients )
		{
			if ( client.Pawn is not Player player ) continue;

			if ( player.CanRespawn() || player.IsAlive )
				teamHash.Add( player.Team.Resource.Id );
		}

		if ( teamHash.Count > 1 ) return;

		GameLoop.FinishWithWin( teamHash.First() );
	}
}
