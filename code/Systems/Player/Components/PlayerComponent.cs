namespace GoldRush;

public class PlayerComponent : EntityComponent<Player>
{
	/// <summary>
	/// Called when a game event is sent to the player.
	/// </summary>
	/// <param name="eventName"></param>
	public virtual void OnGameEvent( string eventName )
	{
		//
	}
}
