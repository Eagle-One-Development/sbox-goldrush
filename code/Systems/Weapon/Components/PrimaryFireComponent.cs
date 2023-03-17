namespace GoldRush.Weapons;

[Prefab]
public partial class PrimaryFire : WeaponComponent, ISingletonComponent
{
	[Net, Prefab, Category( "Damage" )] public float BaseDamage { get; set; }
	[Net, Prefab, Category( "Bullet" )] public float BulletRange { get; set; }
	[Net, Prefab, Category( "Bullet" )] public float BulletForce { get; set; }
	[Net, Prefab, Category( "Bullet" )] public float BulletSize { get; set; }
	[Net, Prefab, Category( "Bullet" )] public float BulletSpread { get; set; }
	[Net, Prefab, Category( "Bullet" )] public int BulletCount { get; set; }
	[Net, Prefab, Category( "Fire" )] public float FireDelay { get; set; }
	[Net, Prefab, Category( "Fire" )] public float SprintToFireDelay { get; set; } = 0.2f;
	[Net, Prefab, ResourceType( "sound" ), Category( "Fire" )] public string FireSound { get; set; }

	[Net, Prefab, Category( "Recoil" )] public Vector2 Recoil { get; set; } = new Vector2( 0, 10 );
	[Net, Prefab, Category( "Recoil" )] public float RecoilTightnessFactor { get; set; } = 5.0f;
	[Net, Prefab, Category( "Recoil" )] public float RecoilRecoveryScaleFactor { get; set; } = 20.0f;

	[Net, Prefab, Category( "View Kick" )] public float ViewKickbackStrength { get; set; } = 10.0f;
	[Net, Prefab, Category( "View Kick" )] public float ViewKickbackRecoveryScaleFactor { get; set; } = 10.0f;

	TimeUntil TimeUntilCanFire { get; set; }

	protected override bool CanStart( Player player )
	{
		if ( !Input.Down( InputButton.PrimaryAttack ) ) return false;
		if ( TimeUntilCanFire > 0 ) return false;
		if ( !Weapon.CanFire( player ) ) return false;

		// check for ammo
		if ( Weapon.Components.TryGet<AmmoComponent>( out var ammo ) && (!ammo.HasAmmo() || ammo.IsReloading) )
			return false;

		return TimeSinceActivated > FireDelay;
	}

	public override void OnGameEvent( string eventName )
	{
		if ( eventName == "sprint mechanic.stop" )
		{
			TimeUntilCanFire = SprintToFireDelay;
		}
	}

	protected override void OnStart( Player player )
	{
		if ( Weapon.Components.TryGet<AmmoComponent>( out var ammo ) && !ammo.TakeAmmo() )
			return;

		base.OnStart( player );

		player?.SetAnimParameter( "b_attack", true );

		var wasHit = ShootBullet( BulletSpread, BulletForce, BulletSize, BulletCount, BulletRange );

		Entity.Recoil += Recoil;
		Entity.ViewKick = 1.0f;

		// Send clientside effects to the player.
		if ( Game.IsServer )
		{
			player.PlaySound( FireSound );
			DoShootEffects( To.Single( player ), wasHit );
		}
	}

	[ClientRpc]
	public static void DoShootEffects( bool wasHit )
	{
		Game.AssertClient();
		WeaponViewModel.Current?.SetAnimParameter( "b_attack", true );

		// Melee weapons contain a "b_hit" parameter that is set to true when the weapon hits something.
		WeaponViewModel.Current?.SetAnimParameter( "b_hit", wasHit );
	}

	public IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 end, float radius )
	{
		var tr = Trace.Ray( start, end )
			.UseHitboxes()
			.WithAnyTags( "solid", "player", "glass" )
			.Ignore( Entity )
			.Size( radius )
			.Run();

		if ( tr.Hit )
		{
			yield return tr;
		}
	}

	/// <returns>Whether this bullet hit anything or not</returns>
	public bool ShootBullet( float spread, float force, float bulletSize, int bulletCount = 1, float bulletRange = 5000f )
	{
		bool wasHit = false;

		//
		// Seed rand using the tick, so bullet cones match on client and server
		//
		Game.SetRandomSeed( Time.Tick );

		for ( int i = 0; i < bulletCount; i++ )
		{
			var rot = Rotation.LookAt( Player.AimRay.Forward );

			var forward = rot.Forward;
			forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
			forward = forward.Normal;

			var damage = BaseDamage;

			foreach ( var tr in TraceBullet( Player.AimRay.Position, Player.AimRay.Position + forward * bulletRange, bulletSize ) )
			{
				tr.Surface.DoBulletImpact( tr );

				if ( !Game.IsServer ) continue;
				if ( !tr.Entity.IsValid() ) continue;

				if ( tr.Hit ) wasHit = true;

				var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 100 * force, damage )
					.UsingTraceResult( tr )
					.WithAttacker( Player )
					.WithWeapon( Weapon );

				tr.Entity.TakeDamage( damageInfo );
			}
		}

		return wasHit;
	}
}
