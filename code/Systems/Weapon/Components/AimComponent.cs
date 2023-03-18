namespace GoldRush.Weapons;

[Prefab]
public partial class Aim : WeaponComponent, ISingletonComponent
{
	TimeUntil _timeUntilCanAim;

	public override void OnGameEvent( string eventName )
	{
		if ( eventName == "ammo component.finished" )
			_timeUntilCanAim = 1.0f;
	}

	protected override bool CanStart( Player player )
	{
		if ( !Input.Down( InputButton.SecondaryAttack ) ) return false;
		if ( !Weapon.CanFire( player ) ) return false;

		// We don't want the player to aim in while reloading
		if ( Weapon.IsReloading ) return false;

		// Don't want the player to aim in immediately after reloading either,
		// as this causes issues with the animgraph
		if ( _timeUntilCanAim > 0 ) return false;

		return true;
	}

	protected override void OnStart( Player player )
	{
		base.OnStart( player );

		player?.SetAnimParameter( "b_aiming", true );

		if ( Game.IsServer )
		{
			ToggleAimEffects( To.Single( player ) );
		}
	}

	protected override void OnStop( Player player )
	{
		base.OnStop( player );

		player?.SetAnimParameter( "b_aiming", false );

		if ( Game.IsServer )
		{
			ToggleAimEffects( To.Single( player ) );
		}
	}

	public override void Simulate( IClient cl, Player player )
	{
		base.Simulate( cl, player );

		if ( Weapon.IsReloading )
			IsActive = false;
	}

	[ClientRpc]
	public static void ToggleAimEffects()
	{
		Game.AssertClient();
		WeaponViewModel.Current?.SetAnimParameter( "b_aim", true );
	}
}
