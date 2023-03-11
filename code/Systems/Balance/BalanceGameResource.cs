namespace GoldRush;

[GameResource( "Gold Rush Balance", "grbal", "Defines balance values for Gold Rush" )]
public partial class BalanceGameResource : GameResource
{
	public Dictionary<string, int> EventRewards { get; set; }
}
