
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

	public void SetState( string identifier )
	{
		Game.AssertServer();

		var state = TypeLibrary.Create<GameState>( identifier );
		if ( state is null )
			Log.Error( $"Failed to create unknown state: {identifier}" );

		if ( ActiveState is not null && ActiveState.IsActive )
			ActiveState.Finish();

		ActiveState = state;

		// enter all current clients into new state
		foreach ( var client in Game.Clients )
		{
			ActiveState.OnClientJoined( client );
		}

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
		if ( Current.ActiveState is null )
			return;

		if ( Current.ActiveState.DisplayInfo.ClassName == "waiting" )
			Current.ActiveState.Finish();
	}
}
