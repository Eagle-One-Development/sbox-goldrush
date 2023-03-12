namespace GoldRush;

[AttributeUsage( AttributeTargets.Method )]
internal class OnGameEventAttribute : Attribute
{
	public string EventName;

	public OnGameEventAttribute( string eventName )
	{
		EventName = eventName;
	}
}
