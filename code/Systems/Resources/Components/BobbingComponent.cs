namespace GoldRush;

[Prefab, Title( "Bobbing Component" )]
public partial class BobbingComponent : EntityComponent
{
	[Prefab, Net] public float Speed { get; set; } = 90f;
	[Prefab, Net] public float Height { get; set; } = 2f;

	[Event.PreRender]
	public virtual void OnPreRender()
	{
		if ( Entity is not ModelEntity { SceneObject: SceneObject sceneObject } )
			return;

		if ( !sceneObject.IsValid() )
			return;

		// Rotate over time 🔁
		sceneObject.Rotation = Rotation.From( 0, Time.Now * Speed, 0 );

		// Bob up and down ↕️
		Vector3 bobbingOffset = Vector3.Up * MathF.Sin( Time.Now ) * Height;
		sceneObject.Position = Entity.Position + bobbingOffset;
	}
}
