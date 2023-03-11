namespace GoldRush;

public class Resources
{
	public static int GetRewardForEvent( string eventName )
	{
		var resource = BalanceGameResource.Instance;
		if ( resource.EventRewards.TryGetValue( eventName, out int reward ) )
			return reward;

		return 0;
	}
}
