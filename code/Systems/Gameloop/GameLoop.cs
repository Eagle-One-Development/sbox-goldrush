
using GoldRush.Nexus;

namespace GoldRush;

public partial class GameLoop : Entity
{
	[Net]
	public GameState ActiveState { get; set; }

	public GameLoop()
	{
		Current = this;
	}

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	public static GameLoop Current;

	public static string State => Current.ActiveState.DisplayInfo.ClassName;

	public static void OnClientJoined( IClient client )
	{
		Current?.ActiveState?.OnClientJoined( client );
	}

	public static void OnClientDisconnect( IClient client, NetworkDisconnectionReason reason )
	{
		Current?.ActiveState?.OnClientDisconnect( client, reason );
	}

	public static void OnPawnKilled( Entity pawn )
	{
		if ( pawn is not Player player ) return;
		Current?.ActiveState?.OnPlayerKilled( player );
	}

	public void SetState( string identifier )
	{
		Game.AssertServer();

		var state = TypeLibrary.Create<GameState>( identifier );
		if ( state is null )
			Log.Error( $"Failed to create unknown state: {identifier}" );

		if ( ActiveState is not null )
		{
			if ( ActiveState.IsActive )
				ActiveState.Finish();

			// enter all current clients into new state
			foreach ( var client in ActiveState.Clients )
			{
				state.OnClientJoined( client );
			}
		}

		ActiveState = state;

		ActiveState.Start();
	}

	[Event.Tick]
	public void Tick()
	{
		DebugOverlay.ScreenText( $"gameloop:" );
		DebugOverlay.ScreenText( $"state: {ActiveState}", 1 );
		DebugOverlay.ScreenText( $"time: {ActiveState?.TimeSinceStarted}", 2 );
		DebugOverlay.ScreenText( $"active: {ActiveState?.IsActive}", 3 );

		if ( ActiveState is null )
			return;

		if ( !ActiveState.IsActive && ActiveState.IsAuthority )
		{
			ActiveState.Delete();
			return;
		}

		ActiveState.Update();
	}

	//[ConCmd.Admin( "gr_gameloop_init" )]
	public static void Init()
	{
		Game.AssertServer();

		_ = new GameLoop();
	}

	//[ConCmd.Admin( "gr_gameloop_start" )]
	public static void StartLoop()
	{
		Game.AssertServer();

		if ( Current is null )
			Init();

		Current.SetState( "waiting" );
	}

	[ConCmd.Admin( "gr_gameloop_skip_waiting" )]
	public static void SkipWaiting()
	{
		Game.AssertServer();

		if ( Current.ActiveState is null )
			return;

		if ( Current.ActiveState.DisplayInfo.ClassName == "waiting" )
			Current.ActiveState.Finish();
	}

	[ConCmd.Admin( "gr_gameloop_reset" )]
	public static void Reset()
	{
		Game.AssertServer();

		Current?.ActiveState?.Delete();

		StartLoop();
	}

	[ConCmd.Admin( "gr_gameloop_restart" )]
	public static void Restart()
	{
		Game.AssertServer();

		// start new waiting period
		Reset();

		// skip to maingame
		SkipWaiting();
	}

	[ConCmd.Admin( "gr_gameloop_respawn_all" )]
	public static void RespawnAll()
	{
		Game.AssertServer();

		NexusEntity.ResetAll();

		foreach ( var client in Game.Clients.Where( x => x.Pawn is Player ) )
		{
			var player = client.Pawn as Player;

			player.Respawn();
		}
	}

	public static void FinishWithWin( string teamId )
	{
		Game.AssertServer();

		if ( Current is null )
			return;

		Event.Run( "gameloop.win" );
		Event.Run( $"gameloop.win.{teamId}" );
		Chat.AddChatEntry( To.Everyone, "GAME", $"Team {teamId} wins!", "0", true );
		Reset();
	}

	[ConCmd.Admin( "gr_gameloop_force_win" )]
	public static void ForceWin()
	{
		FinishWithWin( "null" );
	}
}
