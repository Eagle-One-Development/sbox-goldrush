namespace GoldRush.Teams;

public partial class Resources : EntityComponent<Player>, ISingletonComponent
{
	[Net] public int Gold { get; private set; }

	public Resources()
	{
	}

	public void OnKill()
	{
		Gold += 10;
	}
}
