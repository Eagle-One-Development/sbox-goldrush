namespace GoldRush.Teams;

public partial class Team : EntityComponent<Player>, ISingletonComponent
{
	[Net] public TeamGameResource TeamGameResource { get; set; }

	public Team()
	{
		// We'll set the team up here for now
		TeamGameResource = (Game.Clients.Count % 2 == 0)
			? ResourceLibrary.Get<TeamGameResource>( "data/teams/red.grteam" )
			: ResourceLibrary.Get<TeamGameResource>( "data/teams/blue.grteam" );
	}

	public bool IsFriendly( Player other )
	{
		return (other.Team.TeamGameResource == TeamGameResource);
	}

	public bool IsFriendly( IClient other )
	{
		if ( other.Pawn is Player player )
			return IsFriendly( player );

		return false;
	}

	public bool IsEnemy( Player other )
	{
		return !IsFriendly( other );
	}

	public bool IsEnemy( IClient other )
	{
		if ( other.Pawn is Player player )
			return IsEnemy( player );

		return false;
	}
}
