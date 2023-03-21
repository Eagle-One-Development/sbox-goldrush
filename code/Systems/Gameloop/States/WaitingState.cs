
namespace GoldRush;

[Library( "waiting" )]
public partial class WaitingState : GameState
{
	[ConVar.Server( "gr_gameloop_min_players" )]
	public static int MinimumPlayers { get; set; } = 2;

	[ConVar.Server( "gr_gameloop_waiting_time", Saved = true )]
	public static int WaitingTime { get; set; } = 30;

	[Net]
	private TimeSince _timeSinceMinimumPlayers { get; set; }

	public override void OnFinish()
	{
		base.OnFinish();

		GameLoop.SetState( "maingame" );
	}

	public override void OnClientJoined( IClient client )
	{
		base.OnClientJoined( client );

		if ( Clients.Count == MinimumPlayers )
			_timeSinceMinimumPlayers = 0;

		if ( client.Pawn is Player player )
			player.Respawn();
	}


	public override void Update()
	{
		base.Update();

		DebugOverlay.ScreenText( $"{_timeSinceMinimumPlayers}", 6 );
		DebugOverlay.ScreenText( $"{Clients.Count}", 7 );
		DebugOverlay.ScreenText( $"", 8 );

		if ( Clients.Count < MinimumPlayers )
			return;

		if ( _timeSinceMinimumPlayers >= WaitingTime )
			Finish();
	}
}
