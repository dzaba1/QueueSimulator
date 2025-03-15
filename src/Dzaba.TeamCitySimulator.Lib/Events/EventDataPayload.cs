using System;

namespace Dzaba.TeamCitySimulator.Lib.Events;

internal class EventDataPayload
{
    public EventDataPayload(EventData eventData)
    {
        ArgumentNullException.ThrowIfNull(eventData, nameof(eventData));

        EventData = eventData;
    }

    public EventData EventData { get; }
}
