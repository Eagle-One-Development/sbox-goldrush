namespace GoldRush;

[Prefab, Title( "Bobbing Component" )]
public partial class BobbingComponent : EntityComponent
{
	[Net] public Vector3 PositionOffset { get; set; }

	[Event.PreRender]
	public virtual void OnPreRender()
	{
		if ( Entity is not ModelEntity { SceneObject: SceneObject sceneObject } )
			return;

		if ( !sceneObject.IsValid() )
			return;

		// Rotate over time 🔁
		sceneObject.Rotation = Rotation.From( 0, Time.Now * 90f, 0 );

		// Bob up and down ↕️
		Vector3 bobbingOffset = Vector3.Up * MathF.Sin( Time.Now * 2f );
		sceneObject.Position = Entity.Position + (bobbingOffset + PositionOffset) * Entity.Scale;
	}
}
