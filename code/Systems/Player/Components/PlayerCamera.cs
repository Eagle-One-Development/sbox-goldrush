using GoldRush.Mechanics;

namespace GoldRush;

public partial class PlayerCamera : EntityComponent<Player>, ISingletonComponent
{
	private float _interpolatedFovMultiplier = 1.0f;

	public virtual void Update( Player player )
	{
		float fieldOfView = EvaluateFieldOfView( player );

		Camera.Position = player.EyePosition;
		Camera.Rotation = player.EyeRotation;
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

	/// <summary>
	/// Calculate a field of view based on the player's current state.
	/// This will multiply against the field of view specified in the user's preferences.
	/// </summary>
	private float EvaluateFieldOfView( Player player )
	{
		float fovMultiplier = 1.0f;

		if ( player.Controller.IsMechanicActive<SprintMechanic>() )
			fovMultiplier = 1.1f;

		if ( player.ActiveWeapon.IsAiming )
			fovMultiplier = 0.8f;

		// Interpolate so that FOV transitions smoothly
		_interpolatedFovMultiplier = _interpolatedFovMultiplier.LerpTo( fovMultiplier, 10f * Time.Delta );

		float fieldOfView = Game.Preferences.FieldOfView * _interpolatedFovMultiplier;
		return fieldOfView;
	}
}
