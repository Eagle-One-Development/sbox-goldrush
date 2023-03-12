namespace GoldRush;

public enum AmmoType
{
	Generic,
	Pistol,
	Smg
}

public partial class PlayerAmmo : PlayerComponent, ISingletonComponent
{
	[Net] public IDictionary<AmmoType, int> Ammo { get; set; } = new Dictionary<AmmoType, int>();

	public bool HasAmmo( AmmoType type )
	{
		if ( Ammo.TryGetValue( type, out var ammo ) )
			return ammo > 0;
		return false;
	}

	/// <summary>
	/// Try and take some ammo from the ammo inventory. Returns how much was taken, if any. 
	/// </summary>
	public int TryTakeAmmo( AmmoType type, int amount = 1 )
	{
		if ( TakeAmmo( type, amount ) )
			return amount;

		// weren't able to take full amount, take rest
		var available = GetAmmo( type );
		SubAmmo( type, available );
		return available;
	}

	/// <summary>
	/// Take some ammo. Returns if we were able to take any.
	/// </summary>
	public bool TakeAmmo( AmmoType type, int amount = 1 )
	{
		if ( GetAmmo( type ) < amount ) return false;

		SubAmmo( type, amount );

		return true;
	}

	public int GetAmmo( AmmoType type )
	{
		if ( Ammo.TryGetValue( type, out var ammo ) )
			return ammo;
		return 0;
	}

	public void SetAmmo( AmmoType type, int amount )
	{
		Ammo[type] = amount;
	}

	public void AddAmmo( AmmoType type, int amount )
	{
		if ( Ammo.ContainsKey( type ) )
			Ammo[type] += amount;
		else Ammo[type] = amount;
	}

	public void SubAmmo( AmmoType type, int amount )
	{
		if ( Ammo.ContainsKey( type ) )
			Ammo[type] -= amount;
	}

	[ConCmd.Admin( "gr_give_ammo" )]
	public static void GiveAllAmmo( int amount )
	{
		if ( ConsoleSystem.Caller?.Pawn is not Player player )
			return;

		foreach ( var ammoType in (AmmoType[])Enum.GetValues( typeof( AmmoType ) ) )
		{
			player.Ammo.AddAmmo( ammoType, amount );

			Log.Info( $"Granted {amount} {ammoType} ammo" );
		}
	}
}
