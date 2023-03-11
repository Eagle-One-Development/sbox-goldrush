namespace GoldRush;

[GameResource( "Gold Rush Balance", "grbal", "Defines balance values for Gold Rush" )]
public class BalanceGameResource : GameResource
{
	public Dictionary<string, int> EventRewards { get; set; }

	private static BalanceGameResource s_instance;
	public static BalanceGameResource Instance
	{
		get
		{
			if ( s_instance == null )
				s_instance = ResourceLibrary.Get<BalanceGameResource>( "data/default.grbal" );

			return s_instance;
		}
	}
}
