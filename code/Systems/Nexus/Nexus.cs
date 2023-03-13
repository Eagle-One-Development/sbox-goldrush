using Sandbox;
using Sandbox.Utility;

using System.Collections.Generic;
using Editor;

namespace GoldRush.Nexus;

[Library( "goldrush_nexus" )]
[HammerEntity]
[EditorModel( "models/environment/nexus/spawnnexus.vmdl" )]
[Title( "Nexus" ), Category( "Gameplay" )]
public partial class Nexus : AnimatedEntity
{
	[Property( Title = "The team ID this nexus corresponds to." )]
	public int TeamId { get; set; }

	[Property( "The max health of the nexus" )]
	public int MaxHealth { get; set; }

	[Net]
	public int Health { get; set; }

	public delegate void OnDeathEvent( Nexus nexus );
	public event OnDeathEvent OnDeath;


	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;

	}


	[Event.Frame]
	public void DrawDebugInfo()
	{

	}

	public override void TakeDamage( DamageInfo info )
	{
		base.TakeDamage( info );

		//If the attacker is a player and has a team component
		if ( info.Attacker is Player player && player.Team != null )
		{
			//If the player's team is the same as the nexus's team
			if ( player.Team.Id == TeamId )
			{
				//Don't take damage
				return;
			}
		}

		Health -= info.Damage;
		if ( Health <= 0 )
		{
			OnDeath?.Invoke( this );
			Health = 0;
		}
	}
}
