namespace GoldRush;

[GameResource( "Gold Rush Rewards", "grrew", "Defines rewards for game events" )]
public class RewardsGameResource : GameResource
{
	public Dictionary<string, int> EventRewards { get; set; }

	private static RewardsGameResource s_instance;
	public static RewardsGameResource Instance
	{
		get
		{
			if ( s_instance == null )
				s_instance = ResourceLibrary.Get<RewardsGameResource>( "data/default.grrew" );

			return s_instance;
		}
	}
}
