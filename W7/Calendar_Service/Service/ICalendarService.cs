using System.Collections.Concurrent;
using Ical.Net.DataTypes;
using CalendarEvent = Calendar_Service.Models.CalendarEvent;

namespace Calendar_Service.Service;

public interface ICalendarService
{
    Task<Guid> CreateEventAsync(CalendarEvent calendarEvent);
    Task<CalendarEvent?> GetEventAsync(Guid id);
    IEnumerable<CalendarEvent> GetAllEvents();
    IEnumerable<CalendarEvent> GetOccurrences(DateTimeOffset from, DateTimeOffset to);
}

public class InMemoryCalendarService : ICalendarService
{
    private readonly ConcurrentDictionary<Guid, CalendarEvent> _events = new();
    
    public Task<Guid> CreateEventAsync(CalendarEvent calendarEvent)
    {
        calendarEvent.Id = Guid.NewGuid();
        _events.TryAdd(calendarEvent.Id, calendarEvent);
        return Task.FromResult(calendarEvent.Id); // имитация асинхронности
    }

    public Task<CalendarEvent?> GetEventAsync(Guid id)
    {
        _events.TryGetValue(id, out var evt);
        return Task.FromResult(evt);
    }
    
    public IEnumerable<CalendarEvent> GetAllEvents() => _events.Values;

    public IEnumerable<CalendarEvent> GetOccurrences(DateTimeOffset from, DateTimeOffset to)
    {
        var result = new List<CalendarEvent>();

        foreach (var evt in _events.Values)
        {
            if (string.IsNullOrEmpty(evt.RecurrentRule))
            {
                if(evt.StartTime >= from && evt.StartTime <= to) result.Add(evt);
                continue;
            }
            
            try
            {
                var rrule = new RecurrencePattern(evt.RecurrentRule);
                var calendarEvent = new Ical.Net.CalendarComponents.CalendarEvent
                {
                    Start = new CalDateTime(evt.StartTime.UtcDateTime),
                    Duration = new Ical.Net.DataTypes.Duration(evt.Duration.Days, evt.Duration.Hours, evt.Duration.Minutes, evt.Duration.Seconds),
                    RecurrenceRules = new List<RecurrencePattern> { rrule }
                };
                
                var occurrences = calendarEvent.GetOccurrences(new CalDateTime(from.UtcDateTime));

                foreach (var occurrence in occurrences)
                {
                    result.Add(new CalendarEvent
                    {
                        Id = evt.Id,
                        Title = evt.Title,
                        Duration = evt.Duration,
                        Location = evt.Location,
                        RecurrentRule = evt.RecurrentRule,
                        StartTime = new DateTimeOffset(occurrence.Period.StartTime.AsUtc)
                    });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error parsing rule for {evt.Id}: {e.Message}");
            }
        }

        return result;
    }
}