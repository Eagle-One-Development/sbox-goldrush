namespace GoldRush.Teams;

public interface ITeam : IEquatable<ITeam>
{
	string Id { get; }
	string DisplayName { get; }
	Color Color { get; }
	string ScssClass { get; }
}

public abstract class Team : ITeam
{
	public abstract string Id { get; }
	public abstract string DisplayName { get; }
	public abstract Color Color { get; }
	public abstract string ScssClass { get; }

	public bool Equals( ITeam other )
	{
		if ( other is not ITeam )
			return false;

		return (other.Id == Id);
	}

	public static bool operator ==( Team a, Team b )
	{
		if ( a is null || b is null )
			return false;

		return a.Equals( b );
	}

	public static bool operator !=( Team a, Team b )
	{
		return !(a == b);
	}
}


public class BlueTeam : Team
{
	public override string Id => "blue";
	public override string DisplayName => "Blue Team";
	public override Color Color => Color.Parse( "#0000ff" ) ?? Color.White;
	public override string ScssClass => "team-blue";
}

public class RedTeam : Team
{
	public override string Id => "red";
	public override string DisplayName => "Red Team";
	public override Color Color => Color.Parse( "#ff0000" ) ?? Color.White;
	public override string ScssClass => "team-red";
}
