using Editor;
using GoldRush.Teams;

namespace GoldRush.Nexus;

[Library( "goldrush_nexus" )]
[HammerEntity]
[Model( Model = "models/environment/nexus/spawnnexus.vmdl" )]
[Title( "Nexus" ), Category( "Gameplay" )]
public partial class NexusEntity : AnimatedEntity
{
	[Property, Net]
	public TeamGameResource Team { get; set; }

	[Property( "The max health of the nexus" )]
	public int MaxHealth { get; set; }

	public delegate void OnDeathEvent( NexusEntity nexus );
	public event OnDeathEvent OnDeath;


	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
		LifeState = LifeState.Alive;
		Health = MaxHealth;
		SetupPhysicsFromModel( PhysicsMotionType.Static );
		Tags.Add( "solid", "nexus" );
	}

	[Event.Client.Frame]
	public void Frame()
	{
		if ( SceneObject is null )
			return;

		SceneObject.Batchable = false;
		SceneObject.Attributes.Set( "gr_team_tint", Health > 0 ? Team.Color : Color.Black );
	}

	[Event.Client.Frame]
	public void DrawDebugInfo()
	{
		DebugOverlay.Text(
			$"Team: {Team.Id}\n" +
			$"Health: {Health}\n",
			Position + Vector3.Up * 10,
			Color.White
		);
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

		// bit dirty, maybe get some standard set of enums for main states?
		if ( GameLoop.State != "maingame" )
			return;

		if ( LifeState != LifeState.Alive )
			return;

		Health -= info.Damage;
		if ( Health <= 0 )
		{
			Chat.AddChatEntry( To.Everyone, "GAME", $"The {Team.DisplayName} Nexus has been destroyed! This team can no longer respawn.", "0", true );

			OnDeath?.Invoke( this );
			Health = 0;
			LifeState = LifeState.Dead;
		}
	}

	public static void ResetAll()
	{
		Game.AssertServer();
		foreach ( var nexus in Entity.All.OfType<NexusEntity>() )
		{
			nexus.Health = nexus.MaxHealth;
			nexus.LifeState = LifeState.Alive;
		}
	}
}
