namespace GoldRush.Teams;

public partial class PlayerResources : GameComponent<Player>, ISingletonComponent
{
	[Net] private int NetGold { get; set; }

	public int Gold
	{
		get => NetGold;
		private set => NetGold = value;
	}

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
