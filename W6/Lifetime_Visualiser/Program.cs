using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// --- 1. РЕГИСТРАЦИЯ СЕРВИСОВ ---

builder.Services.AddTransient<ITransientService, Operation>();
builder.Services.AddScoped<IScopedService, Operation>();
builder.Services.AddSingleton<ISingletonService, Operation>();

// ТЕПЕРЬ ЭТО SINGLETON!
// Но мы научим его работать правильно, чтобы не ломать архитектуру.
builder.Services.AddSingleton<ReportService>();

var app = builder.Build();

// --- 2. ENDPOINT ---

app.MapGet("/", (
    ITransientService transient,
    IScopedService scoped,
    ISingletonService singleton,
    ReportService reporter) =>
{
    var directIds = new 
    {
        Transient = transient.OperationId,
        Scoped = scoped.OperationId,
        Singleton = singleton.OperationId
    };

    // ReportService теперь живет вечно.
    // Но внутри он создает временную "карманную вселенную" (Scope), чтобы достать свежие данные.
    var serviceIds = reporter.GetIds();

    return new
    {
        Explanation = "ReportService теперь Singleton. Обрати внимание: Scoped ID внутри него МЕНЯЕТСЯ при каждом запросе, потому что мы создаем Scope вручную.",
        Request_Direct = directIds,
        Inside_Singleton_Service = serviceIds
    };
});

app.Run();

// --- 3. СУЩНОСТИ ---

public interface IOperation { Guid OperationId { get; } }
public interface ITransientService : IOperation { }
public interface IScopedService : IOperation { }
public interface ISingletonService : IOperation { }

public class Operation : ITransientService, IScopedService, ISingletonService
{
    public Guid OperationId { get; } = Guid.NewGuid();
    public Operation() => Console.WriteLine($"[Constructor] Operation created: {OperationId.ToString().Substring(0, 5)}...");
}

// ИСПРАВЛЕННЫЙ СЕРВИС (SINGLETON)
public class ReportService
{
    // Синглтон может безопасно держать ссылки только на другие Синглтоны
    private readonly ISingletonService _singleton;
    
    // А для Scoped/Transient зависимостей нам нужна фабрика
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly ITransientService _transientService;

    public ReportService(
        ISingletonService singleton,
        IServiceScopeFactory scopeFactory,
        ITransientService transientService) // <--- Внедряем Фабрику вместо Scoped-сервиса
    {
        _singleton = singleton;
        _scopeFactory = scopeFactory;
        _transientService = transientService;
    }

    public dynamic GetIds()
    {
        // 1. Создаем "виртуальный кадр" (Scope)
        using var scope = _scopeFactory.CreateScope();
        // 2. Внутри этого блока мы можем безопасно просить Scoped сервисы
        var scopedService = scope.ServiceProvider.GetRequiredService<IScopedService>();
            
        // Transient тоже можно, он просто создастся и умрет вместе со скоупом
        //var transientService = scope.ServiceProvider.GetRequiredService<ITransientService>();

        return new
        {
            // Этот ID будет каждый раз разный, так как мы создаем new Scope()
            Scoped_From_Factory = scopedService.OperationId,
                
            // Этот ID тоже новый
            Transient_From_Factory = _transientService.OperationId,
                
            // А этот вечный, сохранен в поле класса
            Singleton_Direct = _singleton.OperationId
        };
    }
}