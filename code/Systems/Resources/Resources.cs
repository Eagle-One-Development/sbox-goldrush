namespace GoldRush;

public class Resources
{
	public static int GetRewardForEvent( string eventName )
	{
		var resource = BalanceGameResource.Instance;
		if ( resource.EventRewards.TryGetValue( eventName, out int reward ) )
			return reward;

		return 0;
	}

	/// <summary>
	/// Drops gold from the given position.
	/// This will return false if value is 0 or less.
	/// An entity will only be created if the value is greater than 0.
	/// </summary>
	public static bool TryDropFromPosition( Vector3 position, int value, out ModelEntity entity )
	{
		// If this isn't a valid amount of gold, don't spawn & return false.
		if ( value <= 0 )
		{
			entity = null;
			return false;
		}

		entity = PrefabLibrary.Spawn<ModelEntity>( "prefabs/gold.prefab" );

		// Trace down from position to find ground
		var tr = Trace.Ray( position, position + Vector3.Down * 1024f )
			.WithAllTags( "solid" )
			.WithoutTags( "player" )
			.Radius( 8f )
			.Run();

		// If we hit something, spawn the gold there.
		entity.Position = tr.EndPosition;

		return true;
	}
}
