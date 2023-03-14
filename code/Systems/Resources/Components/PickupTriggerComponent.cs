namespace GoldRush;

[Prefab, Title( "Pickup Trigger" )]
public partial class PickupTriggerComponent : EntityComponent
{
	[Prefab, Net] public float Radius { get; set; } = 16f;
	[Prefab, Net] public string EventName { get; set; }

	private PickupTrigger _pickupTrigger;

	protected override void OnActivate()
	{
		if ( !Game.IsServer )
			return;

		_pickupTrigger = new PickupTrigger();
		_pickupTrigger.SetParent( Entity );
		_pickupTrigger.SetTriggerSize( Radius );
		_pickupTrigger.OnTouch += ( e ) =>
		{
			if ( e is Player player )
				player.RunGameEvent( EventName );
		};
	}
}
