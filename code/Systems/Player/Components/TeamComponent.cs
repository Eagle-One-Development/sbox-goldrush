namespace GoldRush.Teams;

public partial class Team : PlayerComponent, ISingletonComponent
{
	[Net] public TeamGameResource TeamGameResource { get; set; }

	public Team()
	{
		// We'll set the team up here for now
		TeamGameResource = (Game.Clients.Count % 2 == 0)
			? ResourceLibrary.Get<TeamGameResource>( "data/teams/red.grteam" )
			: ResourceLibrary.Get<TeamGameResource>( "data/teams/blue.grteam" );
	}

	/// <summary>
	/// Check if another player is friendly with us
	/// </summary>
	public bool IsFriendly( Player other )
	{
		return (other.Team.TeamGameResource == TeamGameResource);
	}

	/// <summary>
	/// Check if another client is friendly with us
	/// </summary>
	public bool IsFriendly( IClient other )
	{
		if ( other.Pawn is Player player )
			return IsFriendly( player );

		return false;
	}

	/// <summary>
	/// Check if another player is enemies with us
	/// </summary>
	public bool IsEnemy( Player other )
	{
		return !IsFriendly( other );
	}

	/// <summary>
	/// Check if another client is enemies with us
	/// </summary>
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
		// Did we just get damaged by a teammate?
		if ( info.Attacker is Player attackerPlayer )
		{
			return IsEnemy( attackerPlayer );
		}

		return true;
	}
}
