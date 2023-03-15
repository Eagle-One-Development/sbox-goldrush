namespace GoldRush.Teams;

public partial class PlayerResources : GameComponent<Player>, ISingletonComponent
{
	[Net] private int NetGold { get; set; }

	public int Gold
	{
		get => NetGold;
		private set
		{
			// We don't want players to get into debt, so we clamp the value to 0
			NetGold = value.Clamp( 0, int.MaxValue );
		}
	}

	public override void OnGameEvent( string eventName )
	{
		// Some events give us gold
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

	[OnGameEvent( "gold.pickup" )]
	private void OnGoldPickup( Entity goldEntity, string quantity )
	{
		goldEntity.Delete();
		Gold += int.Parse( quantity );
	}
}
