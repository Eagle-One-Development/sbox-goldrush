namespace GoldRush.Teams;

public partial class PlayerResources : PlayerComponent, ISingletonComponent
{
	[Net] public int Gold { get; private set; }

	public PlayerResources()
	{
	}

	public override void OnGameEvent( string eventName )
	{
		Gold += GoldRush.Resources.GetRewardForEvent( eventName );
	}
}
