using Editor;
using GoldRush.Teams;

namespace GoldRush.Nexus;

[Library( "goldrush_drill" )]
[HammerEntity]
[Model( Model = "models/environment/drill/drill.vmdl" )]
[Title( "Drill" ), Category( "Gameplay" )]
public partial class Drill : AnimatedEntity
{
	/// <summary>
	/// The time the drill is active for
	/// </summary>
	public float UpTime { get; set; } = 30f;

	/// <summary>
	/// The time the drill is inactive for
	/// </summary>
	public float Cooldown { get; set; } = 5f;

	/// <summary>
	/// The amount of gold the will drop over the UpTime
	/// </summary>
	public int GoldReward { get; set; } = 100;

	override public void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
		SetupPhysicsFromModel( PhysicsMotionType.Static );
		Tags.Add( "solid" );
		Tags.Add( "drill" );
	}



}
