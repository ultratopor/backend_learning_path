# Хранение статических типов и данных в CLR

## Введение

Common Language Runtime (CLR) предоставляет сложную систему для хранения и управления статическими типами и данными. Понимание этих механизмов критически важно для разработки эффективных и надежных .NET приложений.

## 1. Статические типы и члены

### 1.1. Статические поля

**Хранение в памяти:**
- Статические поля хранятся в специальной области памяти, называемой **Loader Heap**
- Эта область управляется CLR и существует на протяжении всего времени жизни домена приложения (AppDomain)
- В отличие от экземплярных полей, статические поля не связаны с конкретным объектом

**Инициализация:**
```csharp
public class MyClass
{
    // Статическое поле с инициализацией при объявлении
    private static int _counter = 0;
    
    // Статический конструктор для сложной инициализации
    static MyClass()
    {
        _counter = LoadInitialValue();
    }
}
```

### 1.2. Статические конструкторы

**Особенности выполнения:**
- Выполняются автоматически перед первым использованием типа
- Гарантируется, что будут выполнены только один раз
- CLR обеспечивает потокобезопасность статических конструкторов

**Порядок выполнения:**
```csharp
public class BaseClass
{
    static BaseClass()
    {
        Console.WriteLine("BaseClass static ctor");
    }
}

public class DerivedClass : BaseClass
{
    static DerivedClass()
    {
        Console.WriteLine("DerivedClass static ctor");
    }
}

// При первом обращении к DerivedClass:
// Output:
// BaseClass static ctor
// DerivedClass static ctor
```

## 2. Типы в CLR

### 2.1. Метаданные типов

**Таблицы метаданных:**
Каждый тип в CLR представлен набором таблиц метаданных:

- **TypeDef**: Определение типа
- **MethodDef**: Определения методов
- **FieldDef**: Определения полей
- **PropertyDef**: Определения свойств
- **EventDef**: Определения событий

**Структура метаданных:**
```csharp
// Упрощенное представление метаданных типа
public class TypeMetadata
{
    public string Name { get; set; }
    public string Namespace { get; set; }
    public TypeFlags Flags { get; set; }
    public Type BaseType { get; set; }
    public MethodMetadata[] Methods { get; set; }
    public FieldMetadata[] Fields { get; set; }
}
```

### 2.2. Загрузка типов

**Lazy Loading (Отложенная загрузка):**
- Типы загружаются только при первом обращении
- JIT-компиляция также происходит по требованию
- Это оптимизирует запуск приложения и использование памяти

**Assembly Load Contexts (.NET Core/.NET 5+):**
```csharp
// Изолированная загрузка сборок
var loadContext = new AssemblyLoadContext("IsolatedContext", isCollectible: true);
var assembly = loadContext.LoadFromAssemblyPath(assemblyPath);
```

## 3. Управление жизненным циклом

### 3.1. AppDomain

**Изоляция приложений:**
- AppDomain обеспечивает изоляцию выполнения кода
- Каждый AppDomain имеет свои статические данные
- В .NET Core AppDomain ограничен по сравнению с .NET Framework

```csharp
// Создание нового AppDomain (только .NET Framework)
var setup = new AppDomainSetup
{
    ApplicationBase = AppDomain.CurrentDomain.BaseDirectory
};

var domain = AppDomain.CreateDomain("IsolatedDomain", null, setup);
```

### 3.2. Сборка мусора и статические данные

**Проблемы со сборкой мусора:**
- Статические поля могут препятствовать сборке мусора
- Циклические ссылки через статические поля создают утечки памяти
- WeakReference может помочь решить некоторые проблемы

```csharp
public class CacheManager
{
    // Использование WeakReference для предотвращения утечек
    private static readonly ConcurrentDictionary<string, WeakReference> _cache 
        = new ConcurrentDictionary<string, WeakReference>();
    
    public static object GetOrCreate(string key, Func<object> factory)
    {
        if (_cache.TryGetValue(key, out var weakRef) && weakRef.TryGetTarget(out var value))
        {
            return value;
        }
        
        value = factory();
        _cache[key] = new WeakReference(value);
        return value;
    }
}
```

## 4. Thread Safety и статические данные

### 4.1. Потокобезопасность статических полей

**Проблемы:**
- Статические поля разделяются между всеми потоками
- Требуется синхронизация при изменении разделяемого состояния

**Решения:**
```csharp
public class ThreadSafeCounter
{
    // Использование Interlocked для атомарных операций
    private static int _counter = 0;
    
    public static int Increment()
    {
        return Interlocked.Increment(ref _counter);
    }
}

public class ThreadSafeCache
{
    // Использование ConcurrentDictionary для потокобезопасного доступа
    private static readonly ConcurrentDictionary<string, object> _cache 
        = new ConcurrentDictionary<string, object>();
    
    public static object GetOrCreate(string key, Func<object> factory)
    {
        return _cache.GetOrAdd(key, factory);
    }
}
```

### 4.2. Lazy Initialization

**Lazy<T> для потокобезопасной инициализации:**
```csharp
public class ExpensiveResource
{
    private static readonly Lazy<ExpensiveResource> _instance 
        = new Lazy<ExpensiveResource>(() => new ExpensiveResource(), LazyThreadSafetyMode.ExecutionAndPublication);
    
    public static ExpensiveResource Instance => _instance.Value;
    
    private ExpensiveResource()
    {
        // Дорогая инициализация
    }
}
```

## 5. Производительность и оптимизация

### 5.1. Generic Types и JIT

**Специализация generic типов:**
- CLR создает специализированные версии generic типов для каждого типа значения
- Для ссылочных типов используется одна специализация
- Это влияет на производительность и размер кода

```csharp
// Для каждого типа значения будет создана своя версия
List<int> intList;     // Специализированная версия
List<double> doubleList; // Специализированная версия

// Для всех ссылочных типов используется одна версия
List<string> stringList;   // Общая версия
List<object> objectList;    // Общая версия
```

### 5.2. Оптимизации статических данных

**Read-only статические поля:**
```csharp
public class Constants
{
    // Компилятор может оптимизировать доступ к readonly полям
    public static readonly double Pi = 3.14159265359;
    public static readonly string DefaultConnection = "Server=localhost;Database=test;";
}
```

**String Interning:**
```csharp
// Строковые литералы автоматически интернируются
string str1 = "Hello";
string str2 = "Hello";
ReferenceEquals(str1, str2); // true

// Явное интернирование
string str3 = string.Intern("Hello");
```

## 6. Диагностика и отладка

### 6.1. Анализ статических данных

**Использование SOS (Son of Strike):**
```
!dumpheap -stat -type System.String
!dumpheap -stat -type System.Collections.Generic.Dictionary
```

**Анализ утечек памяти:**
```
!gcroot <address>
!objsize <address>
```

### 6.2. Performance Counters

**Мониторинг CLR:**
- .NET CLR Memory\# Bytes in all Heaps
- .NET CLR Loading\Current Assemblies
- .NET CLR JIT\% Time in JIT

## 7. Best Practices

### 7.1. Рекомендации по использованию статических данных

1. **Избегайте изменяемого состояния в статических полях**
   - Предпочитайте readonly или immutable структуры

2. **Обеспечивайте потокобезопасность**
   - Используйте ConcurrentDictionary, Lazy<T>, Interlocked

3. **Будьте осторожны с жизненным циклом**
   - Статические поля живут вечно в рамках AppDomain

4. **Используйте dependency injection для тестирования**
   - Статические зависимости затрудняют unit-тестирование

### 7.2. Альтернативные подходы

**Singleton Pattern:**
```csharp
public class ServiceLocator
{
    private static readonly Lazy<ServiceLocator> _instance 
        = new Lazy<ServiceLocator>(() => new ServiceLocator());
    
    public static ServiceLocator Instance => _instance.Value;
    
    private readonly IServiceProvider _serviceProvider;
    
    private ServiceLocator()
    {
        _serviceProvider = ConfigureServices();
    }
    
    public T GetService<T>() => _serviceProvider.GetService<T>();
}
```

## 8. Заключение

Понимание того, как CLR хранит и управляет статическими типами и данными, критически важно для создания эффективных и надежных .NET приложений. Правильное использование статических членов позволяет оптимизировать производительность, но требует внимания к потокобезопасности и управлению жизненным циклом.

## Источники

1. Разработка бэкенда: план обучения C#