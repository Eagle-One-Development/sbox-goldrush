using GoldRush.Weapons;
using Sandbox.Utility;

namespace GoldRush;

public partial class Player
{
	/// <summary>
	/// Should be Input.AnalogMove
	/// </summary>
	[ClientInput] public Vector2 MoveInput { get; protected set; }

	/// <summary>
	/// Normalized accumulation of Input.AnalogLook
	/// </summary>
	[ClientInput] public Angles LookInput { get; protected set; }

	/// <summary>
	/// ?
	/// </summary>
	[ClientInput] public Entity ActiveWeaponInput { get; set; }

	/// <summary>
	/// Position a player should be looking from in world space.
	/// </summary>
	[Browsable( false )]
	public Vector3 EyePosition
	{
		get => Transform.PointToWorld( EyeLocalPosition );
		set => EyeLocalPosition = Transform.PointToLocal( value );
	}

	/// <summary>
	/// Position a player should be looking from in local to the entity coordinates.
	/// </summary>
	[Net, Predicted, Browsable( false )]
	public Vector3 EyeLocalPosition { get; set; }

	/// <summary>
	/// Rotation of the entity's "eyes", i.e. rotation for the camera when this entity is used as the view entity.
	/// </summary>
	[Browsable( false )]
	public Rotation EyeRotation
	{
		get => Transform.RotationToWorld( EyeLocalRotation );
		set => EyeLocalRotation = Transform.RotationToLocal( value );
	}

	/// <summary>
	/// Rotation of the entity's "eyes", i.e. rotation for the camera when this entity is used as the view entity. In local to the entity coordinates.
	/// </summary>
	[Net, Predicted, Browsable( false )]
	public Rotation EyeLocalRotation { get; set; }

	/// <summary>
	/// Override the aim ray to use the player's eye position and rotation.
	/// </summary>
	public override Ray AimRay => new Ray( EyePosition, EyeRotation.Forward );

	public override void BuildInput()
	{
		Inventory?.BuildInput();

		MoveInput = Input.AnalogMove;
		var lookInput = (LookInput + Input.AnalogLook).Normal;

		// This looks bad, but using a fixed deltatime for this makes things feel a lot smoother, even at lower framerates.
		float deltaTime = 1 / 120f;
		// Apply effects from weapons
		ApplyRecoil( ref lookInput, deltaTime );
		ApplyViewKick( ref lookInput, deltaTime );

		// Since we're a FPS game, let's clamp the player's pitch.
		LookInput = lookInput.WithPitch( lookInput.pitch.Clamp( -89f, 89f ) );
	}

	private Vector2 _recoil;

	private void ApplyRecoil( ref Angles lookAngles, float deltaTime )
	{
		var weapon = ActiveWeapon;
		var primaryFire = ActiveWeapon.GetComponent<PrimaryFire>();
		var recoil = weapon.Recoil;

		var prevPitch = lookAngles.pitch;
		var prevYaw = lookAngles.yaw;

		_recoil -= recoil * deltaTime;

		//
		// Apply recoil
		//
		lookAngles.pitch -= recoil.y * deltaTime;
		lookAngles.yaw -= recoil.x * deltaTime;

		ActiveWeapon.Recoil -= weapon.Recoil
			.WithY( (prevPitch - lookAngles.pitch) * primaryFire.RecoilTightnessFactor * 1f )
			.WithX( (prevYaw - lookAngles.yaw) * primaryFire.RecoilTightnessFactor * 1f );

		//
		// Recovery
		//
		var delta = _recoil;
		_recoil = Vector2.Lerp( _recoil, 0, primaryFire.RecoilRecoveryScaleFactor * deltaTime );
		delta -= _recoil;

		lookAngles.pitch -= delta.y;
		lookAngles.yaw -= delta.x;

		_rollMul += weapon.Recoil.Length / 10f;
		_rollMul = _rollMul.LerpTo( 0.0f, 5f * deltaTime );
		_rollMul = _rollMul.Clamp( 0, 1 );
	}

	private float _roll;
	private float _rollMul = 0.0f;

	private void ApplyViewKick( ref Angles lookAngles, float deltaTime )
	{
		var weapon = ActiveWeapon;
		var primaryFire = ActiveWeapon.GetComponent<PrimaryFire>();
		var recoil = weapon.Recoil;

		var rollCoords = recoil + (Time.Now * 100);
		var targetRoll = Noise.Perlin( rollCoords.x, rollCoords.y );

		targetRoll = (-1.0f).LerpTo( 1.0f, targetRoll );
		targetRoll *= primaryFire.ViewKickbackStrength * Easing.BounceInOut( weapon.ViewKick );

		_roll = _roll.LerpTo( targetRoll, 30f * deltaTime );
		lookAngles = new Angles(
			lookAngles.pitch,
			lookAngles.yaw.NormalizeDegrees(),
			_roll
		); ;

		weapon.ViewKick = weapon.ViewKick.LerpTo( 0, primaryFire.ViewKickbackRecoveryScaleFactor * deltaTime );
	}
}
