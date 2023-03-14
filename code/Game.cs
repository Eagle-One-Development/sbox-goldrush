//
//
global using GoldRush.UI;
global using Sandbox;
global using Sandbox.UI;
global using System;
global using System.Collections.Generic;
global using System.ComponentModel;
global using System.Linq;
using GoldRush.Teams;

namespace GoldRush;

public partial class GoldRushGameManager : GameManager
{
	public GoldRushGameManager()
	{
		if ( Game.IsClient )
		{
			Game.RootPanel = new Hud();
		}

		Game.TickRate = 40;
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		// Create a pawn for this client to play with
		var pawn = new Player();
		client.Pawn = pawn;
		pawn.Respawn();

		Chat.AddChatEntry( To.Everyone, client.Name, "joined the game", client.SteamId, true );
	}

	public override void MoveToSpawnpoint( Entity pawn )
	{
		if ( pawn is not Player player )
		{
			base.MoveToSpawnpoint( pawn );
			return;
		}

		var spawnTransform = Transform.Zero;

		if ( !All.OfType<TeamSpawnPoint>().Any() )
		{
			spawnTransform = All.OfType<SpawnPoint>().OrderBy( x => Guid.NewGuid() ).FirstOrDefault().Transform;
		}
		else
		{
			spawnTransform = All.OfType<TeamSpawnPoint>().OrderBy( x => Guid.NewGuid() ).Where( x => x.Team.Id == player.Team.Resource.Id ).FirstOrDefault().Transform;
		}

		spawnTransform = spawnTransform.WithPosition( spawnTransform.Position += Vector3.Up * 10.0f );
		player.Transform = spawnTransform;
		player.ResetInterpolation();
	}

	public override void ClientDisconnect( IClient client, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( client, reason );
		Chat.AddChatEntry( To.Everyone, client.Name, "left the game", client.SteamId, true );
	}
}
