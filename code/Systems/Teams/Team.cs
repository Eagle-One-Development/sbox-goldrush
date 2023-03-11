namespace GoldRush.Teams;

public interface ITeam : IEquatable<ITeam>
{
	string Id { get; }
	string DisplayName { get; }
	Color Color { get; }
	string ScssClass { get; }
}
