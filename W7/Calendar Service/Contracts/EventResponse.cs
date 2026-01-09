namespace Calendar_Service.Contracts;

public sealed record EventResponse(
    Guid Id,
    string Title,
    string Description,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    string Location);