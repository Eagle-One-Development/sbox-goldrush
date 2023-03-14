using Editor;
using GoldRush.Teams;

namespace GoldRush.Nexus;

[Library( "goldrush_nexus" )]
[HammerEntity]
[EditorModel( "models/environment/nexus/spawnnexus.vmdl" )]
[Title( "Nexus" ), Category( "Gameplay" )]
public partial class Nexus : AnimatedEntity
{
	[Property, Net]
	public TeamGameResource Team { get; set; }

	[Property( "The max health of the nexus" )]
	public int MaxHealth { get; set; }

	public delegate void OnDeathEvent( Nexus nexus );
	public event OnDeathEvent OnDeath;


	public override void Spawn()
	{
		base.Spawn();
		SetModel( "models/environment/nexus/spawnnexus.vmdl" );
		Transmit = TransmitType.Always;
		Health = MaxHealth;
		SetupPhysicsFromModel( PhysicsMotionType.Static );
		Tags.Add( "solid" );

	}


	[Event.Client.Frame]
	public void DrawDebugInfo()
	{
		DebugOverlay.Text( $"{Health}", Position + Vector3.Up * 10, Color.White );
		DebugOverlay.Text( $"{Team.Id}", Position + Vector3.Up * 20, Color.White );
	}

	public override void TakeDamage( DamageInfo info )
	{
		//If the attacker is a player and has a team component
		if ( info.Attacker is Player player && player.Team != null )
		{
			//If the player's team is the same as the nexus's team
			if ( player.Team.Resource.Id == Team.Id.ToString() )
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
