using Microsoft.AspNetCore.Mvc;
using Plugin_System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKeyedTransient<IDataExporter, JsonExporter>("json");
builder.Services.AddKeyedTransient<IDataExporter, CsvExporter>("csv");
builder.Services.AddKeyedTransient<IDataExporter, XmlExporter>("xml");

var app = builder.Build();

var matchData = new MatchData("Ranked Game #42", 1500, DateTime.UtcNow);

app.MapPost("/export/{format}", ([FromRoute] string format, [FromServices] IServiceProvider sp) =>
{
    try
    {
        var provider = sp.GetRequiredKeyedService<IDataExporter>(format);
        return Results.Ok(provider.Export(matchData));
    }
    catch (InvalidOperationException)
    {
        return Results.BadRequest($"Format {format} is not supported.");
    }
});

app.Run();

public record MatchData(string Title, int Score, DateTime Date);