namespace GoldRush.Weapons;

public partial class WeaponViewModel
{
	protected ViewModelComponent Data => Weapon.GetComponent<ViewModelComponent>();

	// Fields
	Vector3 SmoothedVelocity;
	Vector3 velocity;
	Vector3 acceleration;
	float VelocityClamp => 20f;
	float walkBob = 0;
	float upDownOffset = 0;
	float avoidance = 0;
	float sprintLerp = 0;
	float crouchLerp = 0;
	float airLerp = 0;
	float sideLerp = 0;

	protected float MouseDeltaLerpX;
	protected float MouseDeltaLerpY;

	Vector3 positionOffsetTarget = Vector3.Zero;
	Rotation rotationOffsetTarget = Rotation.Identity;

	Vector3 realPositionOffset;
	Rotation realRotationOffset;

	float interpolatedFovMultiplier = 1.0f;

	protected void ApplyPositionOffset( Vector3 offset, float delta )
	{
		var left = Camera.Rotation.Left;
		var up = Camera.Rotation.Up;
		var forward = Camera.Rotation.Forward;

		positionOffsetTarget += forward * offset.x * delta;
		positionOffsetTarget += left * offset.y * delta;
		positionOffsetTarget += up * offset.z * delta;
	}

	private float WalkCycle( float speed, float power, bool abs = false )
	{
		var sin = MathF.Sin( walkBob * speed );
		var sign = Math.Sign( sin );

		if ( abs )
		{
			sign = 1;
		}

		return MathF.Pow( sin, power ) * sign;
	}

	private void LerpTowards( ref float value, float desired, float speed )
	{
		var delta = (desired - value) * speed * Time.Delta;
		var deltaAbs = MathF.Min( MathF.Abs( delta ), MathF.Abs( desired - value ) ) * MathF.Sign( delta );

		if ( MathF.Abs( desired - value ) < 0.001f )
		{
			value = desired;

			return;
		}

		value += deltaAbs;
	}

	private void ApplyDamping( ref Vector3 value, float damping )
	{
		var magnitude = value.Length;

		if ( magnitude != 0 )
		{
			var drop = magnitude * damping * Time.Delta;
			value *= Math.Max( magnitude - drop, 0 ) / magnitude;
		}
	}

	public void AddEffects()
	{
		var player = Weapon.Player;
		var controller = player?.Controller;
		if ( controller == null )
			return;

		SmoothedVelocity += (controller.Velocity - SmoothedVelocity) * 5f * Time.Delta;

		var isSprinting = player.IsSprinting;
		var isCrouching = player.IsCrouching;
		var isGrounded = player.IsGrounded;

		var speed = controller.Velocity.WithZ( 0 ).Length.LerpInverse( 0, 500 );
		var bobSpeed = SmoothedVelocity.WithZ( 0 ).Length.LerpInverse( -250, 500 );

		var left = Camera.Rotation.Left;
		var up = Camera.Rotation.Up;
		var forward = Camera.Rotation.Forward;

		LerpTowards( ref sprintLerp, isSprinting ? 1 : 0, 10f );
		LerpTowards( ref crouchLerp, isCrouching ? 1 : 0, 7f );
		LerpTowards( ref airLerp, isGrounded ? 0 : 1, 10f );

		var leftAmt = left.WithZ( 0 ).Normal.Dot( controller.Velocity.Normal );
		LerpTowards( ref sideLerp, leftAmt, 5f );

		bobSpeed += sprintLerp * 0.1f;

		if ( isGrounded )
		{
			walkBob += Time.Delta * 30.0f * bobSpeed;
		}

		walkBob %= 360;

		var mouseDeltaX = -Input.MouseDelta.x * Time.Delta * Data.OverallWeight;
		var mouseDeltaY = -Input.MouseDelta.y * Time.Delta * Data.OverallWeight;

		acceleration += Vector3.Left * mouseDeltaX * -1f;
		acceleration += Vector3.Up * mouseDeltaY * -2f;
		acceleration += -velocity * Data.WeightReturnForce * Time.Delta;

		// Apply horizontal offsets based on walking direction
		var horizontalForwardBob = WalkCycle( 0.5f, 3f ) * speed * Data.WalkCycleOffset.x * Time.Delta;

		acceleration += forward.WithZ( 0 ).Normal.Dot( controller.Velocity.Normal ) * Vector3.Forward * Data.BobAmount.x * horizontalForwardBob;

		// Apply left bobbing and up/down bobbing
		acceleration += Vector3.Left * WalkCycle( 0.5f, 2f ) * speed * Data.WalkCycleOffset.y * (1 + sprintLerp) * Time.Delta;
		acceleration += Vector3.Up * WalkCycle( 0.5f, 2f, true ) * speed * Data.WalkCycleOffset.z * Time.Delta;
		acceleration += left.WithZ( 0 ).Normal.Dot( controller.Velocity.Normal ) * Vector3.Left * speed * Data.BobAmount.y * Time.Delta;

		velocity += acceleration * Time.Delta;

		ApplyDamping( ref acceleration, Data.AccelerationDamping );
		ApplyDamping( ref velocity, Data.WeightDamping );
		velocity = velocity.Normal * Math.Clamp( velocity.Length, 0, VelocityClamp );

		Position = Camera.Position;
		Rotation = Camera.Rotation;

		positionOffsetTarget = Vector3.Zero;
		rotationOffsetTarget = Rotation.Identity;

		{
			//
			// Apply camera effects here so that they aren't affected
			// by sprinting rotation offset etc.
			//
			AddCameraEffects();

			// Global
			rotationOffsetTarget *= Rotation.From( Data.GlobalAngleOffset );
			positionOffsetTarget += forward * (velocity.x * Data.VelocityScale + Data.GlobalPositionOffset.x);
			positionOffsetTarget += left * (velocity.y * Data.VelocityScale + Data.GlobalPositionOffset.y);
			positionOffsetTarget += up * (velocity.z * Data.VelocityScale + Data.GlobalPositionOffset.z + upDownOffset);

			// Crouching
			rotationOffsetTarget *= Rotation.From( Data.CrouchAngleOffset * crouchLerp );
			ApplyPositionOffset( Data.CrouchPositionOffset, crouchLerp );

			// Air
			ApplyPositionOffset( new( 0, 0, 1 ), airLerp );

			// View bobbing
			float bobStrength = speed;
			float cycle = Time.Now * 10.0f;

			Rotation *= Rotation.From(
				MathF.Abs( MathF.Sin( cycle ) * 1.0f ) * bobStrength,
				MathF.Cos( cycle ) * bobStrength,
				0
			);

			// Sprinting Camera Rotation
			Rotation *= Rotation.Lerp( Rotation.Identity, Rotation.From( 10, 10, -10 ), sprintLerp );
		}

		realRotationOffset = rotationOffsetTarget;
		realPositionOffset = positionOffsetTarget;

		Rotation *= realRotationOffset;
		Position += realPositionOffset;

		Camera.Main.SetViewModelCamera( EvaluateFieldOfView(), 1, 2048 );
	}

	private void AddCameraEffects()
	{
		if ( GetBoneIndex( "camera" ) < 0 )
			return;

		Angles baseAngles = new Angles( 90, 90, 0 );

		var animAngles = GetBoneTransform( "camera" ).Rotation.Angles();

		// Swap P and R
		animAngles = animAngles.WithPitch( animAngles.roll ).WithRoll( animAngles.pitch );
		// Subtract base
		animAngles -= baseAngles;

		var modelAngles = Rotation.Angles();
		var delta = modelAngles - animAngles;

		Camera.Main.Rotation *= delta.ToRotation();
	}

	private float EvaluateFieldOfView()
	{
		const float baseFieldOfView = 85f;
		float fovMultiplier = 1.0f;

		if ( Weapon.IsAiming )
			fovMultiplier = 0.8f;

		// Interpolate so that FOV transitions smoothly
		interpolatedFovMultiplier = interpolatedFovMultiplier.LerpTo( fovMultiplier, 10f * Time.Delta );

		float fieldOfView = baseFieldOfView * interpolatedFovMultiplier;
		return fieldOfView;
	}
}
