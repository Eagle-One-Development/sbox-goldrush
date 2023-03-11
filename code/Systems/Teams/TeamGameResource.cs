namespace GoldRush.Teams;

[GameResource( "Gold Rush Team", "grteam", "Defines a team for Gold Rush" )]
public class TeamGameResource : GameResource, ITeam
{
	[Category( "Metadata" )] public string Id { get; set; }
	[Category( "Metadata" )] public string DisplayName { get; set; }
	[Category( "Metadata" )] public Color Color { get; set; }
	[Category( "Metadata" )] public string ScssClass { get; set; }

	public bool Equals( ITeam other )
	{
		if ( other is not ITeam )
			return false;

		return (other.Id == Id);
	}

	public static bool operator ==( TeamGameResource a, TeamGameResource b )
	{
		if ( a is null || b is null )
			return false;

		return a.Equals( b );
	}

	public static bool operator !=( TeamGameResource a, TeamGameResource b )
	{
		return !(a == b);
	}
}
