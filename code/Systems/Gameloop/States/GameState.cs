
namespace GoldRush;

public partial class GameState : Entity
{
	[Net]
	public TimeSince TimeSinceStarted { get; set; }
	[Net]
	public bool IsActive { get; set; }
	public GameLoop GameLoop => GameLoop.Current;

	[Net]
	public IList<IClient> Clients { get; set; } = new List<IClient>();

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	public void Start()
	{
		TimeSinceStarted = 0;
		IsActive = true;
		OnStart();
	}

	public virtual void OnStart() { }

	public void Finish()
	{
		IsActive = false;
		OnFinish();
	}

	public virtual void OnFinish() { }

	public virtual void Update() { }

	public virtual void OnClientJoined( IClient client )
	{
		Clients.Add( client );
	}

	public virtual void OnClientDisconnect( IClient client, NetworkDisconnectionReason reason )
	{
		Clients.Remove( client );
	}

	public virtual void OnPlayerKilled( Player player )
	{

	}

	DisplayInfo? displayInfo;
	public DisplayInfo DisplayInfo
	{
		get
		{
			displayInfo ??= DisplayInfo.For( this );
			return displayInfo.Value;
		}
	}
}
