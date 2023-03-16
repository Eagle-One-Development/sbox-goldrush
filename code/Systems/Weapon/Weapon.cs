namespace GoldRush.Weapons;

[Prefab, Title( "Weapon" ), Icon( "track_changes" )]
public partial class Weapon : AnimatedEntity
{
	// Won't be Net eventually, when we serialize prefabs on client
	[Net, Prefab, Category( "Animation" )] public WeaponHoldType HoldType { get; set; } = WeaponHoldType.Pistol;
	[Net, Prefab, Category( "Animation" )] public WeaponHandedness Handedness { get; set; } = WeaponHandedness.Both;
	[Net, Prefab, Category( "Animation" )] public float HoldTypePose { get; set; } = 0;

	public AnimatedEntity EffectEntity => ViewModelEntity.IsValid() ? ViewModelEntity : this;
	public WeaponViewModel ViewModelEntity { get; protected set; }
	public Player Player => Owner as Player;

	private Aim AimComponent { get; }
	public bool IsAiming => AimComponent?.IsActive ?? false;
	public bool IsReloading => Components.Get<AmmoComponent>()?.IsReloading ?? false;

	public override void Spawn()
	{
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		EnableDrawing = false;
	}

	/// <summary>
	/// Is the player in a state where they can fire a weapon?
	/// </summary>
	public bool CanFire( Player player )
	{
		return !player.IsSprinting;
	}

	/// <summary>
	/// Is the player in a state where they can reload a weapon?
	/// </summary>
	public bool CanReload( Player player )
	{
		return !player.IsSprinting;
	}

	/// <summary>
	/// Can we holster the weapon right now? Reasons to reject this could be that we're reloading the weapon..
	/// </summary>
	public bool CanHolster( Player player )
	{
		return true;
	}

	/// <summary>
	/// Called when the weapon gets holstered.
	/// </summary>
	public void OnHolster( Player player )
	{
		EnableDrawing = false;

		if ( Game.IsServer )
			DestroyViewModel( To.Single( player ) );
	}

	/// <summary>
	/// Can we deploy this weapon? Reasons to reject this could be that we're performing an action.
	/// </summary>
	public bool CanDeploy( Player player )
	{
		return true;
	}

	/// <summary>
	/// Called when the weapon gets deployed.
	/// </summary>
	public void OnDeploy( Player player )
	{
		SetParent( player, true );
		Owner = player;

		EnableDrawing = true;

		if ( Game.IsServer )
			CreateViewModel( To.Single( player ) );
	}

	[ClientRpc]
	public void CreateViewModel()
	{
		if ( GetComponent<ViewModelComponent>() is not ViewModelComponent comp ) return;

		var vm = new WeaponViewModel( this );
		vm.Model = Model.Load( comp.ViewModelPath );
		ViewModelEntity = vm;
	}

	[ClientRpc]
	public void DestroyViewModel()
	{
		if ( ViewModelEntity.IsValid() )
		{
			ViewModelEntity.Delete();
		}
	}

	public override void Simulate( IClient cl )
	{
		SimulateComponents( cl );
	}

	protected override void OnDestroy()
	{
		ViewModelEntity?.Delete();
	}

	public override string ToString()
	{
		return $"Weapon ({Name})";
	}
}

/// <summary>
/// Describes the holdtype of a weapon, which tells our animgraph which animations to use.
/// </summary>
public enum WeaponHoldType
{
	None,
	Pistol,
	Rifle,
	Shotgun,
	Item,
	Fists,
	Swing
}

/// <summary>
/// Describes the handedness of a weapon, which hand (or both) we hold the weapon in.
/// </summary>
public enum WeaponHandedness
{
	Both,
	Right,
	Left
}

