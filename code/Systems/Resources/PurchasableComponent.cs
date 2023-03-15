namespace GoldRush;

public enum PurchasableCategory
{
	Weapon,
	Upgrade,
	Consumable
}

[Prefab]
public partial class Purchasable : EntityComponent, ISingletonComponent
{
	[Net, Prefab] public int Cost { get; set; } = 69;
	[Net, Prefab] public PurchasableCategory Category { get; set; } = PurchasableCategory.Weapon;
	[Net, Prefab] public int Quantity { get; set; } = 1;
}
