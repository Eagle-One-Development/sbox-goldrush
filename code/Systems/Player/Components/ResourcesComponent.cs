namespace GoldRush.Teams;

public partial class Resources : PlayerComponent, ISingletonComponent
{
	[Net] public int Gold { get; private set; }

	public Resources()
	{
	}

	public override void OnGameEvent( string eventName )
	{
		Gold += Rewards.GetRewardForEvent( eventName );
	}
}
