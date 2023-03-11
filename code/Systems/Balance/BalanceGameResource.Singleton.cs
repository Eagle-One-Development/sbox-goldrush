namespace GoldRush;

partial class BalanceGameResource
{

	private static BalanceGameResource s_instance;
	public static BalanceGameResource Instance
	{
		get
		{
			s_instance ??= ResourceLibrary.Get<BalanceGameResource>( "data/default.grbal" );

			return s_instance;
		}
	}
}
