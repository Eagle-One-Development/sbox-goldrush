namespace GoldRush.Weapons;

[Prefab]
public partial class AmmoComponent : WeaponComponent, ISingletonComponent
{
	// config
	[Net, Prefab] public int ClipSize { get; set; } = 10;
	[Net, Prefab] public AmmoType Type { get; set; } = AmmoType.Generic;
	[Net, Prefab] public float ReloadTime { get; set; } = 1f;
	[Net, Prefab] public float SprintToReloadDelay { get; set; } = 0.2f;
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
		if ( eventName == "sprint mechanic.stop" )
		{
			TimeUntilCanReload = SprintToReloadDelay;
		}
	}

	protected override bool CanStart( Player player )
	{
		if ( !player.Ammo.HasAmmo( Type ) ) return false;
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
		WeaponViewModel.Current?.SetAnimParameter( "b_reload", true );
	}

	[ClientRpc]
	public static void StopReloadEffects()
	{
		Game.AssertClient();
		WeaponViewModel.Current?.SetAnimParameter( "deploy", true );
	}

	public override void Simulate( IClient cl, Player player )
	{
		base.Simulate( cl, player );

		//
		// Check for reload exit conditions
		//
		if ( IsReloading )
		{
			if ( !Weapon.CanReload( player ) )
				ForceEndReload( player );

			if ( TimeSinceActivated > ReloadTime )
				FinishReload();
		}

		// ammo dropping
		if ( player.Ammo.HasAmmo( Type ) && Input.Pressed( InputButton.Drop ) && Game.IsServer )
		{
			var ammo = player.Ammo.TryTakeAmmo( Type, ClipSize );
			DropAmmo( player, ammo );
		}
	}

	private void FinishReload()
	{
		var ammo = Player.Ammo.TryTakeAmmo( Type, ClipSize - Ammo );
		Ammo += ammo;

		IsReloading = false;
	}

	private void ForceEndReload( Player player )
	{
		//
		// Even though we disable player sprinting, we should bail in case that
		// check fails somewhere and the player does manage to sprint.
		//
		IsReloading = false;
		StopReloadEffects( To.Single( player ) );
	}

	public void DropAmmo( Player player, int amount )
	{
		Game.AssertServer();

		var ent = new AmmoEntity();
		ent.Type = Type;
		ent.Amount = amount;

		ent.Position = player.EyePosition;
		ent.Velocity = player.EyeRotation.Forward * 250;
		ent.ResetInterpolation();
	}

	partial class AmmoEntity : ModelEntity
	{
		public AmmoType Type;
		public int Amount;
		public TimeSince TimeSinceDropped;

		private Particles Effect;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/sbox_props/burger_box/burger_box.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
			Tags.Add( "ammo", "prop", "trigger" );

			Effect = Particles.Create( "particles/pickup.vpcf" );
			Effect.SetEntity( 0, this );

			EnableDrawing = true;
			EnableAllCollisions = true;
			EnableTouch = true;

			TimeSinceDropped = 0;
		}

		public override void StartTouch( Entity other )
		{
			base.StartTouch( other );

			if ( !Game.IsServer )
				return;

			if ( other is not Player player )
				return;

			if ( TimeSinceDropped <= 1f )
				return;

			player.Ammo.AddAmmo( Type, Amount );
			Effect?.Destroy( true );
			Delete();
		}
	}
}

public static class WeaponAmmoExtension
{
	public static bool HasAmmo( this Weapon weapon )
	{
		if ( weapon is null ) return false;
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
		if ( weapon is null ) return AmmoType.Generic;
		if ( !weapon.Components.TryGet<AmmoComponent>( out var component ) )
			throw new Exception( $"Cannot get AmmoType for weapon without AmmoComponent!" );

		return component.Type;
	}
}
