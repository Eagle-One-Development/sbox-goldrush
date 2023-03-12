namespace GoldRush.Teams;

public partial class PlayerResources : GameComponent<Player>, ISingletonComponent
{
	[Net] public int Gold { get; private set; }

	public override void OnGameEvent( string eventName )
	{
		Gold += Resources.GetRewardForEvent( eventName );
	}

	[OnGameEvent( "player.waskilled" )]
	private void OnKilled()
	{
		if ( !Game.IsServer )
			return;

		// Drop gold when killed 🤑
		Resources.TryDropFromPosition( Entity.EyePosition, 100, out _ );
	}
}
