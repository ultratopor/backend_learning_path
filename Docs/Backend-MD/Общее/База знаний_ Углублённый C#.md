# База знаний: Углублённый C#

## Введение

Этот документ представляет собой обобщение ключевых концепций и продвинутых техник программирования на C#, которые формируют фундамент для разработки высокопроизводительных Enterprise-систем. Материал структурирован для систематизации знаний и быстрой справки при решении сложных архитектурных задач.

## 1. CLR и Runtime Internals

### 1.1. Компиляция и выполнение

**Двухэтапная компиляция:**

1. **C# → IL (Intermediate Language):** Исходный код компилируется в промежуточный язык, независимый от платформы.
2. **JIT-компиляция:** IL преобразуется в машинный код во время выполнения (Just-In-Time).

**Преимущества IL:**
- Платформенная независимость
- Валидация кода перед выполнением
- Оптимизации на основе рантайм-информации

### 1.2. Управление памятью

**Сегментированная модель кучи:**

- **SOH (Small Object Heap):** Объекты < 85,000 байт
- **LOH (Large Object Heap):** Объекты ≥ 85,000 байт
- **POH (Pinned Object Heap):** Закрепленные объекты (новое в .NET 5+)

**Поколения сборщика мусора:**
- **Gen 0:** Новые объекты, собираются чаще всего
- **Gen 1:** Объекты, пережившие сборку Gen 0
- **Gen 2:** Долгоживущие объекты, собираются реже всего

### 1.3. Типы и значения

**Value Types vs Reference Types:**

```csharp
// Value Type (структура)
struct Point
{
    public int X, Y;
}

// Reference Type (класс)
class Rectangle
{
    public Point TopLeft, BottomRight;
}
```

**Боксинг и анбоксинг:**
```csharp
int i = 42;          // Value type
object o = i;        // Boxing (копирование в heap)
int j = (int)o;      // Unboxing (копирование обратно)
```

## 2. Продвинутые возможности языка

### 2.1. Generics и ковариантность/контравариантность

**Ковариантность (out):**
```csharp
IEnumerable<Derived> derived = new List<Derived>();
IEnumerable<Base> base = derived; // Ковариантность
```

**Контравариантность (in):**
```csharp
Action<Base> baseAction = b => Console.WriteLine(b);
Action<Derived> derivedAction = baseAction; // Контравариантность
```

### 2.2. Делегаты и события

**Объявление делегата:**
```csharp
public delegate void OperationCompleted(int result);
```

**Многоадресные делегаты:**
```csharp
OperationCompleted multiOp = null;
multiOp += LogResult;
multiOp += SendNotification;
multiOp(42); // Вызовет оба метода
```

### 2.3. LINQ и Expression Trees

**LINQ запросы:**
```csharp
var adults = users
    .Where(u => u.Age >= 18)
    .OrderBy(u => u.Name)
    .Select(u => new { u.Name, u.Age });
```

**Expression Trees:**
```csharp
Expression<Func<User, bool>> predicate = u => u.IsActive;
// Можно анализировать и модифицировать дерево выражений
```

## 3. Асинхронное программирование

### 3.1. async/await

**Базовый синтаксис:**
```csharp
public async Task<string> GetDataAsync()
{
    var client = new HttpClient();
    var response = await client.GetAsync("https://api.example.com");
    return await response.Content.ReadAsStringAsync();
}
```

**ConfigureAwait:**
```csharp
var result = await SomeOperationAsync().ConfigureAwait(false);
// Не захватывать контекст синхронизации
```

### 3.2. ValueTask

**Оптимизация для синхронных операций:**
```csharp
public ValueTask<int> GetValueAsync()
{
    // Если значение уже доступно
    if (_cached)
        return _value; // Без аллокации Task
    
    return GetValueFromSourceAsync();
}
```

## 4. Продвинутые паттерны

### 4.1. Repository и Unit of Work

**Интерфейс Repository:**
```csharp
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}
```

### 4.2. CQRS (Command Query Responsibility Segregation)

**Команда:**
```csharp
public class CreateUserCommand : IRequest<int>
{
    public string Name { get; set; }
    public string Email { get; set; }
}
```

**Обработчик команды:**
```csharp
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, int>
{
    public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Логика создания пользователя
        return userId;
    }
}
```

### 4.3. Specification Pattern

**Интерфейс спецификации:**
```csharp
public interface ISpecification<T>
{
    Expression<Func<T, bool>> ToExpression();
}
```

**Пример спецификации:**
```csharp
public class ActiveUserSpecification : ISpecification<User>
{
    public Expression<Func<User, bool>> ToExpression()
    {
        return user => user.IsActive && user.LastLogin > DateTime.UtcNow.AddDays(-30);
    }
}
```

## 5. Производительность и оптимизация

### 5.1. Memory Pooling

**ArrayPool<T>:**
```csharp
var pool = ArrayPool<byte>.Shared;
byte[] buffer = pool.Rent(1024);
try
{
    // Использование буфера
}
finally
{
    pool.Return(buffer);
}
```

### 5.2. Span<T> и Memory<T>

**Span<T>:**
```csharp
public void ProcessData(ReadOnlySpan<byte> data)
{
    // Работаем с частью массива без копирования
    var header = data.Slice(0, 4);
    var payload = data.Slice(4);
}
```

### 5.3. Source Generators

**Генерация кода во время компиляции:**
```csharp
[Generator]
public class MySourceGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        // Генерация кода на основе анализа
    }
}
```

## 6. Reflection и метапрограммирование

### 6.1. Базовое отражение

**Получение типа:**
```csharp
Type type = typeof(MyClass);
// или
Type type = obj.GetType();
```

**Получение членов типа:**
```csharp
PropertyInfo[] properties = type.GetProperties();
MethodInfo[] methods = type.GetMethods();
```

### 6.2. Динамическая генерация кода

**Expression Trees для динамических запросов:**
```csharp
public static IQueryable<T> WhereDynamic<T>(
    this IQueryable<T> source, 
    string propertyName, 
    object value)
{
    var param = Expression.Parameter(typeof(T), "x");
    var property = Expression.Property(param, propertyName);
    var constant = Expression.Constant(value);
    var equality = Expression.Equal(property, constant);
    var lambda = Expression.Lambda<Func<T, bool>>(equality, param);
    
    return source.Where(lambda);
}
```

## 7. Безопасность

### 7.1. Криптография

**Хеширование паролей:**
```csharp
public string HashPassword(string password)
{
    return BCrypt.Net.BCrypt.HashPassword(password);
}

public bool VerifyPassword(string password, string hash)
{
    return BCrypt.Net.BCrypt.Verify(password, hash);
}
```

### 7.2. Защита от атак

**SQL Injection Prevention:**
```csharp
// Плохо (уязвимо)
string sql = $"SELECT * FROM Users WHERE Name = '{userName}'";

// Хорошо (безопасно)
string sql = "SELECT * FROM Users WHERE Name = @userName";
var users = await connection.QueryAsync<User>(sql, new { userName });
```

## 8. Тестирование

### 8.1. Unit Testing

**Arrange-Act-Assert Pattern:**
```csharp
[Fact]
public void Add_ShouldReturnCorrectSum()
{
    // Arrange
    var calculator = new Calculator();
    int a = 5, b = 3;
    
    // Act
    int result = calculator.Add(a, b);
    
    // Assert
    Assert.Equal(8, result);
}
```

### 8.2. Mocking

**Использование Moq:**
```csharp
var mockRepository = new Mock<IRepository<User>>();
mockRepository
    .Setup(r => r.GetByIdAsync(1))
    .ReturnsAsync(new User { Id = 1, Name = "Test User" });

var service = new UserService(mockRepository.Object);
var user = await service.GetUserById(1);
```

## 9. Диагностика и отладка

### 9.1. Логирование

**Structured Logging с Serilog:**
```csharp
Log.Information("User {UserId} performed {Action} on {Resource}", 
    userId, action, resource);
```

### 9.2. Метрики

**System.Diagnostics.Metrics:**
```csharp
var meter = new Meter("MyApplication");
var counter = meter.CreateCounter<int>("requests_processed");

counter.Add(1, new KeyValuePair<string, object>("endpoint", "/api/users"));
```

## 10. Заключение

Этот документ представляет собой основу для глубокого понимания C# и .NET. Продвинутые концепции, представленные здесь, являются строительными блоками для создания масштабируемых, производительных и надежных Enterprise-систем.

Мастерство этих тем позволяет разработчику принимать обоснованные архитектурные решения и эффективно решать сложные задачи в области разработки программного обеспечения.

## Источники

1. Разработка бэкенда: план обучения C#