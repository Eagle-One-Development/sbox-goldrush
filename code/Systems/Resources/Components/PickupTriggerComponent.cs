﻿namespace GoldRush;

[Prefab, Title( "Pickup Trigger" )]
public partial class PickupTriggerComponent : EntityComponent
{
	[Prefab, Net] public float Radius { get; set; } = 16f;
	[Prefab, Net] public string EventName { get; set; }
	[Prefab, Net] public List<string> EventParameters { get; set; }

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
			{
				var parameters = new List<object>
				{
					Entity
				};

				parameters.AddRange( EventParameters );

				player.RunGameEvent( EventName, parameters.ToArray() );
			}
		};
	}
}
