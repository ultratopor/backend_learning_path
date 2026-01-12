using AutoMapper;
using Calendar_Service.Contracts;
using Calendar_Service.Models;
using Calendar_Service.Service;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Calendar_Service.Controllers;
[ApiController]
[Route(("api/v1/events"))]
public class EventsController(ICalendarService service, IMapper mapper, IValidator<CreateEventRequest> validator) : ControllerBase
{
    private readonly ICalendarService _service = service;
    private readonly IMapper _mapper = mapper;

    [HttpPost]
    [Idempotent]
    public async Task<IActionResult> Create(CreateEventRequest request)
    {
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        
        // Dto -> Entity
        var entity = _mapper.Map<CalendarEvent>(request);
        
        // service layer
        await _service.CreateEventAsync(entity);

        // Entity -> Dto response
        var response = _mapper.Map<EventResponse>(entity);
        
        // формирование ответа
        return CreatedAtAction(nameof(Get), new {id = response.Id}, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var evt = await _service.GetEventAsync(id);
        if (evt == null) return NotFound(new ProblemDetails
        {
            Title = "Event not found",
            Detail = $"Event with id {id} not exist.",
            Status = StatusCodes.Status404NotFound
        });
        
        var response = _mapper.Map<EventResponse>(evt);
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (pageSize > 100) pageSize = 100;

        var events = _service.GetAllEvents()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(events);
    }
    
    [HttpGet("occurrences")]
    public IActionResult GetOccurrences(
        [FromQuery] DateTimeOffset from, 
        [FromQuery] DateTimeOffset to)
    {
        // Валидация: не давай запрашивать календарь за 100 лет, сервер зависнет!
        if ((to - from).TotalDays > 365)
            return BadRequest("Range too wide. Max 1 year.");

        var events = _service.GetOccurrences(from, to);
    
        // Маппим в Response
        var response = _mapper.Map<IEnumerable<EventResponse>>(events);
        return Ok(response);
    }
}