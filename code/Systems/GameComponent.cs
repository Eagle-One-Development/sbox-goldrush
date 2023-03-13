namespace GoldRush;

public interface IGameComponent
{
	/// <summary>
	/// Called when a game event is sent to the player.
	/// </summary>
	/// <param name="eventName"></param>
	void OnGameEvent( string eventName );
}

public class GameComponent<T> : EntityComponent<T>, IGameComponent where T : Entity
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
