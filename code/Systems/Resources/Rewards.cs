namespace GoldRush;

public class Rewards
{
	public static int GetRewardForEvent( string eventName )
	{
		var resource = RewardsGameResource.Instance;
		if ( resource.EventRewards.TryGetValue( eventName, out int reward ) )
			return reward;

		return 0;
	}
}
