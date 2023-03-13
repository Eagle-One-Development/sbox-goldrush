using GoldRush.Weapons;
using Sandbox.Diagnostics;

namespace GoldRush;

public partial class Player
{
	[ConVar.Replicated( "gr_debug_gameevents" )]
	public static bool DebugGameEvents { get; set; } = false;

	static string s_realm = Game.IsServer ? "server" : "client";
	static Logger s_eventLogger = new Logger( $"player/GameEvent/{s_realm}" );

	/// <summary>
	/// Runs a game event on all components that apply to this player.
	/// </summary>
	public void RunGameEvent( string eventName )
	{
		eventName = eventName.ToLowerInvariant();

		var components = CollectComponents();
		components.ForEach( x => RunEventOnComponent( eventName, x ) );

		if ( DebugGameEvents )
			DebugGameEvent( eventName );
	}

	/// <summary>
	/// Collects all of the game components that apply to this player: weapon components,
	/// controller components, and top-level player components.
	/// </summary>
	private List<IGameComponent> CollectComponents()
	{
		var components = new List<IGameComponent>();

		var weaponComponents = Inventory?.ActiveWeapon?.Components.GetAll<WeaponComponent>();
		var controllerComponents = Controller?.Mechanics;
		var playerComponents = PlayerComponents;

		if ( weaponComponents != null )
			components.AddRange( Inventory?.ActiveWeapon?.Components.GetAll<WeaponComponent>() );

		if ( controllerComponents != null )
			components.AddRange( Controller?.Mechanics );

		if ( playerComponents != null )
			components.AddRange( PlayerComponents );

		return components;
	}

	/// <summary>
	/// Invokes OnGameEvent on a given component as well as methods marked with 
	/// the [OnGameEvent] attribute using the given event name.
	/// </summary>
	/// <param name="eventName"></param>
	/// <param name="target"></param>
	private void RunEventOnComponent( string eventName, IGameComponent target )
	{
		if ( target == null )
			return;

		target.OnGameEvent( eventName );

		// This is probably slow as balls, we will likely need to swap this out for
		// something more performant later.
		TypeLibrary.GetType( target.GetType() )
			.Methods
			.Where( x => x.GetCustomAttribute<OnGameEventAttribute>()?.EventName == eventName )
			.ToList()
			.ForEach( x => x.Invoke( target ) );
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
