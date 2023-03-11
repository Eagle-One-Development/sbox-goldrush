//
//
global using GoldRush.UI;
global using Sandbox;
global using Sandbox.UI;
global using System;
global using System.Collections.Generic;
global using System.ComponentModel;
global using System.Linq;

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

		// Get all of the spawnpoints
		var spawnpoints = Entity.All.OfType<SpawnPoint>();

		// chose a random one
		var randomSpawnPoint = spawnpoints.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

		// if it exists, place the pawn there
		if ( randomSpawnPoint != null )
		{
			var tx = randomSpawnPoint.Transform;
			tx.Position = tx.Position + Vector3.Up * 10.0f; // raise it up
			pawn.Transform = tx;
		}

		Chat.AddChatEntry( To.Everyone, client.Name, "joined the game", client.SteamId, true );
	}

	public override void ClientDisconnect( IClient client, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( client, reason );
		Chat.AddChatEntry( To.Everyone, client.Name, "left the game", client.SteamId, true );
	}
}
