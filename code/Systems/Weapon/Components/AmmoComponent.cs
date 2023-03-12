namespace GoldRush.Weapons;

[Prefab]
public partial class AmmoComponent : WeaponComponent, ISingletonComponent
{
	// config
	[Net, Prefab] public int ClipSize { get; set; } = 10;
	[Net, Prefab] public AmmoType Type { get; set; } = AmmoType.Generic;
	[Net, Prefab] public float ReloadTime { get; set; } = 1f;
	[Net, Prefab, ResourceType( "sound" )] public string ReloadSound { get; set; } = "weapons/rust_smg/sounds/rust_smg.reloadstart.sound";

	// active values
	[Net, Predicted] public int Ammo { get; set; }

	[Net, Predicted] public bool IsReloading { get; set; }

	TimeUntil TimeUntilCanReload { get; set; }

	public bool HasAmmo()
	{
		return Ammo > 0;
	}

	public bool TakeAmmo( int amount = 1 )
	{
		if ( Ammo < amount ) return false;

		Ammo -= amount;

		return true;
	}

	protected override void OnActivate()
	{
		base.OnActivate();

		Ammo = ClipSize;
	}

	public override void OnGameEvent( string eventName )
	{
		if ( eventName == "sprint.stop" )
		{
			TimeUntilCanReload = 0.2f;
		}
	}

	protected override bool CanStart( Player player )
	{
		if ( Ammo >= ClipSize ) return false;
		if ( !Input.Down( InputButton.Reload ) ) return false;
		if ( TimeUntilCanReload > 0 ) return false;
		if ( !Weapon.CanReload( player ) ) return false;

		return TimeSinceActivated > 1.0f;
	}

	protected override void OnStart( Player player )
	{
		base.OnStart( player );

		player?.SetAnimParameter( "b_reload", true );

		if ( Game.IsServer )
		{
			player.PlaySound( ReloadSound );
			DoReloadEffects( To.Single( player ) );
		}

		IsReloading = true;
	}

	[ClientRpc]
	public static void DoReloadEffects()
	{
		Game.AssertClient();
		WeaponViewModel.Current?.SetAnimParameter( "reload", true );
	}

	public override void Simulate( IClient cl, Player player )
	{
		base.Simulate( cl, player );

		if ( IsReloading && TimeSinceActivated > ReloadTime )
			FinishReload();
	}

	private void FinishReload()
	{
		var ammo = Player.Ammo.TryTakeAmmo( Type, ClipSize - Ammo );
		Ammo += ammo;

		IsReloading = false;
	}
}

public static class WeaponAmmoExtension
{
	public static bool HasAmmo( this Weapon weapon )
	{
		return weapon.Components.TryGet<AmmoComponent>( out var _ );
	}

	public static int GetAmmo( this Weapon weapon )
	{
		if ( weapon is null ) return 0;
		if ( !weapon.Components.TryGet<AmmoComponent>( out var component ) )
			return 0;

		return component.Ammo;
	}

	public static AmmoType GetAmmoType( this Weapon weapon )
	{
		if ( !weapon.Components.TryGet<AmmoComponent>( out var component ) )
			throw new Exception( $"Cannot get AmmoType for weapon without AmmoComponent!" );

		return component.Type;
	}
}
