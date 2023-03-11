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

	/// <summary>
	/// Checks whether to apply damage to <c>Entity</c> (for example, if the entity is on the same team
	/// as the attacker, this function will return false).
	/// </summary>
	/// <returns><c>true</c> if the entity should take damage, <c>false</c> if the entity shouldn't</returns>
	public bool ShouldTakeDamage( DamageInfo info )
	{
		// TODO: We should probably implement TakeDamage on the team component and go through that here?
		// Did we just get damaged by a teammate?
		if ( info.Attacker is Player player )
		{
			// The player that attacked us was a teammate, so we'll ignore the damage.
			if ( player.Team.IsFriendly( Entity ) )
			{
				Log.Info( $"Attacked by teammate {info.Attacker}" );
				return false;
			}

			// Not a teammate, show a log message so that we know the team system works
			Log.Info( $"Attacked by enemy {info.Attacker}" );
		}

		return true;
	}
}
