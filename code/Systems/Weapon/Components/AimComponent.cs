namespace GoldRush.Weapons;

[Prefab]
public partial class Aim : WeaponComponent, ISingletonComponent
{
	protected override bool CanStart( Player player )
	{
		if ( !Input.Down( InputButton.SecondaryAttack ) ) return false;
		if ( !Weapon.CanFire( player ) ) return false;

		// We don't want the player to aim in while reloading
		if ( Weapon.IsReloading ) return false;

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

	[ClientRpc]
	public static void ToggleAimEffects()
	{
		Game.AssertClient();
		WeaponViewModel.Current?.SetAnimParameter( "b_aim", true );
	}
}
