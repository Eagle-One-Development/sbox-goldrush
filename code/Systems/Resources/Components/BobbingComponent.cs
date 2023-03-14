namespace GoldRush;

[Prefab, Title( "Bobbing Component" )]
public partial class BobbingComponent : EntityComponent
{
	[Prefab, Net] public float RotationSpeed { get; set; } = 90f;
	[Prefab, Net] public float HeightSpeed { get; set; } = 90f;
	[Prefab, Net] public float Height { get; set; } = 2f;

	[Net] public float Offset { get; set; } = 0f;

	protected override void OnActivate()
	{
		Offset = Game.Random.Float( 0f, 360f );
	}

	[Event.PreRender]
	public virtual void OnPreRender()
	{
		if ( Entity is not ModelEntity { SceneObject: SceneObject sceneObject } )
			return;

		if ( !sceneObject.IsValid() )
			return;

		float time = (Time.Now + Offset);

		// Rotate over time 🔁
		sceneObject.Rotation = Rotation.From( 0, time * RotationSpeed, 0 );

		// Bob up and down ↕️
		Vector3 bobbingOffset = Vector3.Up * MathF.Sin( time * HeightSpeed ) * Height;
		sceneObject.Position = Entity.Position + bobbingOffset;
	}
}
