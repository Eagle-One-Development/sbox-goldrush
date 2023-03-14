using Editor;

namespace GoldRush.Teams;

[Library( "info_team_start" )]
[HammerEntity]
[EditorModel( "models/editor/playerstart.vmdl", "white", "white", FixedBounds = true )]
[Title( "Team Spawnpoint" )]
[Category( "Player" )]
[Icon( "place" )]
public class TeamSpawnPoint : Entity
{
	[Property]
	public TeamGameResource Team { get; set; }
}
