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


	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;

	}
}
