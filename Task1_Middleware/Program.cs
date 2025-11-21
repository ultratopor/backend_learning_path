using Task1_Middleware.Models;

var builder = WebApplication.CreateBuilder(args);
// РЕГИСТРАЦИЯ СЕРВИСА
builder.Services.AddSingleton(new List<Item> { new() { Id = 1, Name = "one", Price = 10m }, new() { Id = 2, Name = "two", Price = 11m } });
builder.Services.AddControllers();

var app = builder.Build();
// НАШ MIDDLEWARE ИДЕТ ПЕРЕД ЭНДПОИНТАМИ
app.Use(async (context, next) =>
{
    // [1] Действие ДО: логгирование входящего запроса
    Console.WriteLine($"[LOG - ДО] Запрос получен: {context.Request.Path}");

    await next(); // Передача управления дальше (к app.MapGet)

    // [2] Действие ПОСЛЕ: логгирование после получения ответа
    Console.WriteLine($"[LOG - ПОСЛЕ] Обработка завершена. Статус ответа: {context.Response.StatusCode}");
});

app.MapControllers();

// Эндпоинты, которые наш Middleware должен оборачивать
app.MapGet("/", () => "Hello World!");

app.Run();
