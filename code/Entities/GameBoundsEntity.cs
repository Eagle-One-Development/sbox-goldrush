using Editor;

namespace GoldRush.Entities;

[HammerEntity, Solid]
public class GameBoundsEntity : BaseTrigger
{
	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "gamebounds" );
	}

	public override void OnTouchEnd( Entity toucher )
	{
		base.OnTouchEnd( toucher );

		if ( toucher is not Player player )
			return;

		player.TakeDamage( DamageInfo.Generic( 1000f ) );
		player.RunGameEvent( "player.outofbounds" );
	}
}
