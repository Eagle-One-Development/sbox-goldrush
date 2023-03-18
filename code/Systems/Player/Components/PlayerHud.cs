namespace GoldRush;

public partial class PlayerHud : GameComponent<Player>, ISingletonComponent
{
	[OnGameEvent( "gold.pickup" )]
	public void OnGoldPickup()
	{
		EventFeed.AddEvent( To.Single( Entity ), "Picked up gold", 69 );
	}

	[OnGameEvent( "player.gotkill" )]
	public void OnPlayerGotKill( string playerName )
	{
		EventFeed.AddEvent( To.Single( Entity ), $"Eliminated {playerName}", 0 );
	}

	[OnGameEvent( "player.diddamage" )]
	public void OnPlayerDidDamage()
	{
		Hitmarker.AddHitmarker( To.Single( Entity ) );
	}
}
