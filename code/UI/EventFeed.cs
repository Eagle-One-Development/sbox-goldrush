namespace GoldRush;

public partial class EventFeed
{
	[ConCmd.Client( "gr_add_event", CanBeCalledFromServer = true )]
	public static void AddEvent( string text, int value )
	{
		Instance._events.Add( new Entry( text, value ) );

		// Group events with the same text
		for ( int i = Instance._events.Count - 2; i >= 0; i-- )
		{
			if ( Instance._events[i].Text == text )
			{
				var entry = new Entry( text, Instance._events[i].Value );
				entry.Value += value;
				Instance._events.RemoveAt( Instance._events.Count - 1 );
				Instance._events[i] = entry;
				Instance._events.Sort( ( a, b ) => b.Value.CompareTo( a.Value ) );
				break;
			}
		}
	}
}
