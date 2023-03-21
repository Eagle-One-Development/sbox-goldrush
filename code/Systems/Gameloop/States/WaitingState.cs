
namespace GoldRush;

[Library( "waiting" )]
public class WaitingState : GameState
{
	public override void OnFinish()
	{
		base.OnFinish();

		GameLoop.SetState( "maingame" );
	}

	public override void OnClientJoined( IClient client )
	{
		base.OnClientJoined( client );

		if ( client.Pawn is Player player )
			player.Respawn();
	}

	public override void Update()
	{
		base.Update();

		if ( TimeSinceStarted >= 60 )
			Finish();
		//Log.Info( $"waiting update" );
	}
}
