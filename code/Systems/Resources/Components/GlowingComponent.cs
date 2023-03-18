namespace GoldRush;

[Prefab, Title( "Glowing Component" )]
public class GlowingComponent : EntityComponent
{
	private Particles Effect;

	protected override void OnActivate()
	{
		Effect = Particles.Create( "particles/pickup.vpcf" );
		Effect.SetEntity( 0, Entity );
	}
}
