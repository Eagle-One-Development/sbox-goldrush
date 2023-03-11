namespace GoldRush;

public partial class PlayerCamera : EntityComponent<Player>, ISingletonComponent
{
	public virtual void Update( Player player )
	{
		Camera.Position = player.EyePosition;
		Camera.Rotation = player.EyeRotation;
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );
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
}
