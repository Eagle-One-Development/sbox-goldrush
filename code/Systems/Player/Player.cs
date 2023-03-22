using GoldRush.Mechanics;
using GoldRush.Teams;
using GoldRush.Weapons;

namespace GoldRush;

public partial class Player : AnimatedEntity
{
	/// <summary>
	/// The controller is responsible for player movement and setting up EyePosition / EyeRotation.
	/// </summary>
	[BindComponent] public PlayerController Controller { get; }

	/// <summary>
	/// The animator is responsible for animating the player's current model.
	/// </summary>
	[BindComponent] public PlayerAnimator Animator { get; }

	/// <summary>
	/// The inventory is responsible for storing weapons for a player to use.
	/// </summary>
	[BindComponent] public Inventory Inventory { get; }

	/// <summary>
	/// The player's camera.
	/// </summary>
	[BindComponent] public PlayerCamera Camera { get; }

	/// <summary>
	/// The player's team.
	/// </summary>
	[BindComponent] public PlayerTeam Team { get; }

	/// <summary>
	/// The player's resources (gold etc.)
	/// </summary>
	[BindComponent] public PlayerResources Resources { get; }

	/// <summary>
	/// The player's ammo.
	/// </summary>
	[BindComponent] public PlayerAmmo Ammo { get; }

	/// <summary>
	/// A list of components used by the player.
	/// </summary>
	public IEnumerable<GameComponent<Player>> PlayerComponents => Components.GetAll<GameComponent<Player>>();

	/// <summary>
	/// Accessor for getting a player's active weapon.
	/// </summary>
	public Weapon ActiveWeapon => Inventory?.ActiveWeapon;

	/// <summary>
	/// The information for the last piece of damage this player took.
	/// </summary>
	public DamageInfo LastDamage { get; protected set; }

	/// <summary>
	/// How long since the player last played a footstep sound.
	/// </summary>
	public TimeSince TimeSinceFootstep { get; protected set; } = 0;

	/// <summary>
	/// The model your player will use.
	/// </summary>
	static Model PlayerModel = Model.Load( "models/citizen/citizen.vmdl" );

	/// <summary>
	/// Is the player currently sprinting?
	/// </summary>
	public bool IsSprinting => Controller.IsMechanicActive<SprintMechanic>();

	/// <summary>
	/// Is the player currently crouching?
	/// </summary>
	public bool IsCrouching => Controller.IsMechanicActive<CrouchMechanic>();

	/// <summary>
	/// Is the player on the ground?
	/// </summary>
	public bool IsGrounded => Controller.GroundEntity != null;

	/// <summary>
	/// How much gold does this player have?
	/// </summary>
	public int Gold => Resources.Gold;

	public bool IsAlive => LifeState == LifeState.Alive;

	/// <summary>
	/// When the player is first created. This isn't called when a player respawns.
	/// </summary>
	public override void Spawn()
	{
		Model = PlayerModel;
		Predictable = true;

		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 72 ) );

		// Default properties
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		EnableLagCompensation = true;

		EnableDrawing = false;
		EnableHitboxes = false;
		EnableAllCollisions = false;

		Tags.Add( "player" );

		// Add permanent components
		Components.Create<PlayerTeam>();
		Components.Create<PlayerResources>();
		Components.Create<PlayerAmmo>();
		Components.Create<PlayerHud>();
	}

	public bool CanRespawn()
	{
		if ( !All.OfType<Nexus.NexusEntity>().Where( x => x.Health > 0 && x.Team.Id == Team.Resource.Id ).Any() )
		{
			return false;
		}

		return true;
	}

	/// <summary>
	/// Called when a player respawns, think of this as a soft spawn - we're only reinitializing transient data here.
	/// </summary>
	public void Respawn()
	{
		Health = 100;
		LifeState = LifeState.Alive;

		EnableDrawing = true;
		EnableHitboxes = true;
		EnableAllCollisions = true;

		// Re-enable all children.
		Children.OfType<ModelEntity>()
			.ToList()
			.ForEach( x => x.EnableDrawing = true );

		// We need a player controller to work with any kind of mechanics.
		Components.Create<PlayerController>();

		// Remove old mechanics.
		Components.RemoveAny<PlayerControllerMechanic>();

		// Add mechanics.
		Components.Create<WalkMechanic>();
		Components.Create<JumpMechanic>();
		Components.Create<AirMoveMechanic>();
		Components.Create<SprintMechanic>();
		Components.Create<CrouchMechanic>();
		Components.Create<InteractionMechanic>();

		Components.Create<PlayerAnimator>();
		Components.Create<PlayerCamera>();

		GiveLoadout();

		SetupClothing();

		GameManager.Current?.MoveToSpawnpoint( this );
		ResetInterpolation();
	}

	public void GiveLoadout()
	{
		var inventory = Components.Create<Inventory>();
		inventory.Clear();

		inventory.AddWeapon( PrefabLibrary.Spawn<Weapon>( "prefabs/pickaxe.prefab" ) );
		inventory.AddWeapon( PrefabLibrary.Spawn<Weapon>( "prefabs/pistol.prefab" ), false );
		inventory.AddWeapon( PrefabLibrary.Spawn<Weapon>( "prefabs/smg.prefab" ), false );
		inventory.AddWeapon( PrefabLibrary.Spawn<Weapon>( "prefabs/rifle.prefab" ), false );
		inventory.AddWeapon( PrefabLibrary.Spawn<Weapon>( "prefabs/shotgun.prefab" ), false );

		Ammo.Clear();

		Ammo.AddAmmo( AmmoType.Generic, 1000 );
		Ammo.AddAmmo( AmmoType.Pistol, 100 );
		Ammo.AddAmmo( AmmoType.Smg, 300 );
		Ammo.AddAmmo( AmmoType.Rifle, 300 );
		Ammo.AddAmmo( AmmoType.Shotgun, 100 );
	}

	/// <summary>
	/// Called every server and client tick.
	/// </summary>
	/// <param name="cl"></param>
	public override void Simulate( IClient cl )
	{
		Rotation = LookInput.WithPitch( 0f ).ToRotation();

		Controller?.Simulate( cl );
		Animator?.Simulate( cl );
		Inventory?.Simulate( cl );
	}

	/// <summary>
	/// Called every frame clientside.
	/// </summary>
	/// <param name="cl"></param>
	public override void FrameSimulate( IClient cl )
	{
		Rotation = LookInput.WithPitch( 0f ).ToRotation();

		Controller?.FrameSimulate( cl );
		Camera?.Update( this );
	}

	[ClientRpc]
	public void SetAudioEffect( string effectName, float strength, float velocity = 20f, float fadeOut = 4f )
	{
		Audio.SetEffect( effectName, strength, velocity: 20.0f, fadeOut: 4.0f * strength );
	}

	public override void TakeDamage( DamageInfo info )
	{
		if ( LifeState != LifeState.Alive )
			return;

		if ( !Team.ShouldTakeDamage( info ) )
			return;

		// Check for headshot damage
		var isHeadshot = info.Hitbox.HasTag( "head" );
		if ( isHeadshot )
		{
			info.Damage *= 2.5f;
		}

		// Check if we got hit by a bullet, if we did, play a sound.
		if ( info.HasTag( "bullet" ) )
		{
			Sound.FromScreen( To.Single( Client ), "sounds/player/damage_taken_shot.sound" );
		}

		// Play a deafening effect if we get hit by blast damage.
		if ( info.HasTag( "blast" ) )
		{
			SetAudioEffect( To.Single( Client ), "flashbang", info.Damage.LerpInverse( 0, 60 ) );
		}

		this.ProceduralHitReaction( info );

		if ( Health <= 0 || info.Damage <= 0 ) return;

		Health -= info.Damage;

		LastAttacker = info.Attacker;
		LastAttackerWeapon = info.Weapon;
		LastDamage = info;

		bool isKill = Health <= 0;

		if ( isKill )
			OnKilled();

		if ( info.Attacker is not Player attackingPlayer )
			return;

		if ( !Game.IsServer )
			return;

		if ( isKill )
		{
			RunGameEvent( "player.waskilled" );
			attackingPlayer.RunGameEvent( "player.gotkill", Client.Name );
		}

		attackingPlayer.RunGameEvent( "player.diddamage", isKill );
	}

	public async void RespawnAsync( float delay = 5f )
	{
		await GameTask.DelaySeconds( delay );

		if ( !CanRespawn() )
			return;

		Respawn();
	}

	public override void OnKilled()
	{
		if ( LifeState == LifeState.Alive )
		{
			LifeState = LifeState.Dead;

			CreateRagdoll( Controller.Velocity, LastDamage.Position, LastDamage.Force,
				LastDamage.BoneIndex, LastDamage.HasTag( "bullet" ), LastDamage.HasTag( "blast" ) );

			GameManager.Current?.OnKilled( this );
			EnableAllCollisions = false;
			EnableDrawing = false;

			Controller.Remove();
			Animator.Remove();
			Inventory.Remove();
			Camera.Remove();

			// Disable all children as well.
			Children.OfType<ModelEntity>()
				.ToList()
				.ForEach( x => x.EnableDrawing = false );

			RespawnAsync();
		}
	}

	/// <summary>
	/// Called clientside every time we fire the footstep anim event.
	/// </summary>
	public override void OnAnimEventFootstep( Vector3 pos, int foot, float volume )
	{
		if ( !Game.IsClient )
			return;

		if ( LifeState != LifeState.Alive )
			return;

		if ( TimeSinceFootstep < 0.2f )
			return;

		volume *= GetFootstepVolume();

		TimeSinceFootstep = 0;

		var tr = Trace.Ray( pos, pos + Vector3.Down * 20 )
			.Radius( 1 )
			.Ignore( this )
			.Run();

		if ( !tr.Hit ) return;

		tr.Surface.DoFootstep( this, tr, foot, volume );
	}

	protected float GetFootstepVolume()
	{
		return Controller.Velocity.WithZ( 0 ).Length.LerpInverse( 0.0f, 200.0f ) * 1f;
	}

	[ConCmd.Server( "kill" )]
	public static void DoSuicide()
	{
		(ConsoleSystem.Caller.Pawn as Player)?.TakeDamage( DamageInfo.Generic( 1000f ) );
	}

	[ConCmd.Admin( "sethp" )]
	public static void SetHP( float value )
	{
		(ConsoleSystem.Caller.Pawn as Player).Health = value;
	}
}
