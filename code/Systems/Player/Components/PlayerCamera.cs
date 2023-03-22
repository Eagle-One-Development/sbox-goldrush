using GoldRush.Mechanics;
using GoldRush.Weapons;
using Sandbox.Utility;

namespace GoldRush;

public partial class PlayerCamera : EntityComponent<Player>, ISingletonComponent
{
	private float _interpolatedFovMultiplier = 1.0f;

	public virtual void Update( Player player )
	{
		float fieldOfView = EvaluateFieldOfView( player );

		//
		// Apply effects from weapons
		//
		// This might look bad - but using a fixed deltatime for this makes things feel a lot smoother, even at lower framerates.
		float deltaTime = 1f / 120f;
		var lookAngles = player.EyeRotation.Angles();
		ApplyViewKick( ref lookAngles, deltaTime );

		Camera.Position = player.EyePosition;
		Camera.Rotation = lookAngles.ToRotation();
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( fieldOfView );
		Camera.FirstPersonViewer = player;
		Camera.ZNear = 0.5f;

		// Post Processing
		var pp = Camera.Main.FindOrCreateHook<Sandbox.Effects.ScreenEffects>();
		pp.Sharpen = 0.05f;
		pp.MotionBlur.Scale = 0f;
		pp.Saturation = 1f;
		pp.FilmGrain.Response = 1f;
		pp.FilmGrain.Intensity = 0.01f;
	}

	private Angles _roll;

	private void ApplyViewKick( ref Angles lookAngles, float deltaTime )
	{
		var weapon = Entity.ActiveWeapon;

		if ( !weapon.IsValid() )
			return;

		var primaryFire = Entity.ActiveWeapon.GetComponent<PrimaryFire>();

		if ( primaryFire == null )
			return;

		var targetRoll = weapon.ViewKick;
		_roll = _roll.LerpTo( targetRoll, primaryFire.ViewKickbackTightnessFactor * deltaTime );

		//
		// Applies bounce to a component (P/Y/R)
		//
		float ApplyBounce( float component )
		{
			// We take the sign out because Easing.BounceInOut isn't really compatible with it
			float absRoll = MathF.Abs( component );
			float t = Easing.BounceInOut( absRoll );
			float kickAngle = primaryFire.ViewKickbackStrength * t;

			// Multiply by the sign so that we get left/right
			kickAngle *= MathF.Sign( component );

			return kickAngle;
		}

		var kickAngles = new Angles(
			ApplyBounce( _roll.pitch ) * 0.1f,
			ApplyBounce( _roll.yaw ) * 0.1f,
			ApplyBounce( _roll.roll ) * 1.0f
		);

		lookAngles += kickAngles;

		weapon.ViewKick = weapon.ViewKick.LerpTo( Angles.Zero, primaryFire.ViewKickbackRecoveryScaleFactor * deltaTime );
	}

	/// <summary>
	/// Calculate a field of view based on the player's current state.
	/// This will multiply against the field of view specified in the user's preferences.
	/// </summary>
	private float EvaluateFieldOfView( Player player )
	{
		float fovMultiplier = 1.0f;

		if ( player.Controller.IsMechanicActive<SprintMechanic>() )
			fovMultiplier = 1.1f;

		if ( player.ActiveWeapon?.IsAiming ?? false )
			fovMultiplier = 0.8f;

		// Interpolate so that FOV transitions smoothly
		_interpolatedFovMultiplier = _interpolatedFovMultiplier.LerpTo( fovMultiplier, 10f * Time.Delta );

		float fieldOfView = Game.Preferences.FieldOfView * _interpolatedFovMultiplier;
		return fieldOfView;
	}
}
