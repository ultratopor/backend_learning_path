using Calendar_Service;
using Calendar_Service.Service;
using FluentValidation;
using Microsoft.OpenApi; // Для AutoMapper профилей, если они там

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. РЕГИСТРАЦИЯ СЕРВИСОВ (DI Container)
// Это как "Inventory" движка. Мы кладем сюда инструменты.
// ==========================================

// Добавляем поддержку контроллеров (мы же не используем Minimal API пока)
builder.Services.AddControllers();

// --- SWAGGER SETUP (Генератор документации) ---
// Учит приложение понимать, какие у нас есть эндпоинты
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c => 
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Calendar API", Version = "v1" });
});

// --- AUTOMAPPER ---
// Сканирует текущую сборку (Program) и ищет все классы Profile
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddMemoryCache();

// --- НАШИ СЕРВИСЫ ---
// Singleton: один экземпляр на всю жизнь приложения
builder.Services.AddSingleton<ICalendarService, InMemoryCalendarService>();

var app = builder.Build();

// ==========================================
// 2. MIDDLEWARE PIPELINE (Конвейер запроса)
// Это как порядок отрисовки или физики. Порядок важен!
// ==========================================

// Включаем Swagger. 
// В продакшене часто убирают под if (app.Environment.IsDevelopment()), 
// но пока учимся — оставим включенным всегда, чтобы ты его видел.
app.UseSwagger(); 
app.UseSwaggerUI(c => 
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Calendar API V1");
    // Эта настройка делает так, что Swagger открывается в корне (localhost:5xxx/), а не /swagger
    // c.RoutePrefix = string.Empty; 
});

// Перенаправление на HTTPS (не обязательно для локальной разработки, но полезно)
app.UseHttpsRedirection();

// Подключаем маршрутизацию контроллеров
app.MapControllers();

app.Run();