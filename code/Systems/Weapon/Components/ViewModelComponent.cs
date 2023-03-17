namespace GoldRush.Weapons;

[Prefab]
public partial class ViewModelComponent : WeaponComponent, ISingletonComponent
{
	// I know that there's a metric fuck ton of Net properties here..
	// ideally, when the prefab gets set up, we'd send the client a message with the prefab's name
	// so we can populate all the Prefab marked properties with their defaults.

	//// General
	[Net, Prefab, ResourceType( "vmdl" ), Category( "General" )] public string ViewModelPath { get; set; }

	[Net, Prefab, Category( "General" )] public float OverallWeight { get; set; }
	[Net, Prefab, Category( "General" )] public float WeightReturnForce { get; set; }
	[Net, Prefab, Category( "General" )] public float WeightDamping { get; set; }
	[Net, Prefab, Category( "General" )] public float AccelerationDamping { get; set; }
	[Net, Prefab, Category( "General" )] public float VelocityScale { get; set; }

	//// Walking & Bob
	[Net, Prefab, Category( "Walking & Bob" )] public Vector3 WalkCycleOffset { get; set; }
	[Net, Prefab, Category( "Walking & Bob" )] public Vector2 BobAmount { get; set; }

	//// Global
	[Net, Prefab, Category( "Global" )] public Vector3 GlobalPositionOffset { get; set; }
	[Net, Prefab, Category( "Global" )] public Angles GlobalAngleOffset { get; set; }

	//// Crouching
	[Net, Prefab, Category( "Crouching" )] public Vector3 CrouchPositionOffset { get; set; }
	[Net, Prefab, Category( "Crouching" )] public Angles CrouchAngleOffset { get; set; }

	//// Sprinting
	[Net, Prefab, Category( "Sprinting" )] public Vector3 SprintPositionOffset { get; set; }
	[Net, Prefab, Category( "Sprinting" )] public Angles SprintAngleOffset { get; set; }
}
