using GoldRush.Weapons;
using Sandbox.Diagnostics;

namespace GoldRush;

public partial class Player
{
	[ConVar.Replicated( "gr_debug_gameevents" )]
	public static bool DebugGameEvents { get; set; } = false;

	static string s_realm = Game.IsServer ? "server" : "client";
	static Logger s_eventLogger = new Logger( $"player/GameEvent/{s_realm}" );

	public void RunGameEvent( string eventName )
	{
		eventName = eventName.ToLowerInvariant();

		Inventory?.ActiveWeapon?.Components.GetAll<WeaponComponent>()
			.ToList()
			.ForEach( x => x.OnGameEvent( eventName ) );

		Controller?.Mechanics.ToList()
			.ForEach( x => x.OnGameEvent( eventName ) );

		PlayerComponents?.ToList()
			.ForEach( x => x.OnGameEvent( eventName ) );

		if ( DebugGameEvents )
			DebugGameEvent( eventName );
	}

	int _debugLine;
	static int s_maxLines = 10;

	private void DebugGameEvent( string eventName )
	{
		// Print to console
		s_eventLogger.Trace( $"{s_realm}: {eventName}" );

		// Display on screen too
		float duration = 1f;
		Vector2 position = new( 100, 100 );
		int debugLineOffset = Game.IsServer ? 0 : s_maxLines + 5;

		int line = debugLineOffset + _debugLine;
		string text = $"{s_realm}: {eventName}";

		DebugOverlay.ScreenText( text, position, line, Color.White, duration );

		// Increment line
		_debugLine = (_debugLine + 1) % s_maxLines;
	}
}
