# Dictionary<TKey, TValue>: Глубокое погружение

## Введение

`Dictionary<TKey, TValue>` — одна из наиболее используемых коллекций в .NET, предоставляющая эффективную реализацию хеш-таблицы для хранения пар ключ-значение. Понимание внутренней структуры и особенностей работы словаря критически важно для создания высокопроизводительных приложений.

## 1. Внутренняя структура

### 1.1. Хеш-таблица

**Основные компоненты:**
```csharp
// Упрощенная структура Dictionary
public class Dictionary<TKey, TValue>
{
    private struct Entry
    {
        public int hashCode;      // Хеш-код ключа
        public int next;         // Индекс следующей записи в цепочке
        public TKey key;         // Ключ
        public TValue value;      // Значение
    }
    
    private Entry[] entries;     // Массив записей
    private int[] buckets;      // Массив бакетов (индексов)
    private int count;           // Количество элементов
}
```

**Принцип работы:**
1. Вычисление хеш-кода ключа
2. Определение бакета через `hashCode % buckets.Length`
3. Поиск в цепочке записей для нужного ключа

### 1.2. Разрешение коллизий

**Separate Chaining:**
```csharp
// При коллизии создается связный список в бакете
bucket[index] = newEntryIndex;
newEntry.next = previousEntryIndex;
```

**Пример коллизии:**
```csharp
var dict = new Dictionary<string, int>();
dict.Add("cat", 1);  // hashCode % buckets = 5
dict.Add("dog", 2);  // hashCode % buckets = 5 (коллизия!)
// dog будет добавлена в цепочку после cat
```

## 2. Производительность

### 2.1. Сложность операций

| Операция | Средняя сложность | Худшая сложность | Примечания |
|---|---|---|---|
| Add | O(1) | O(n) | При необходимости ресайза |
| Remove | O(1) | O(n) | При поиске элемента |
| TryGetValue | O(1) | O(n) | При множестве коллизий |
| ContainsKey | O(1) | O(n) | При множестве коллизий |

### 2.2. Факторы влияющие на производительность

**Качество хеш-функции:**
```csharp
// Плохая хеш-функция (много коллизий)
public class BadKey
{
    public int Value { get; set; }
    
    public override int GetHashCode()
    {
        return Value % 10; // Только 10 возможных хешей
    }
}

// Хорошая хеш-функция
public class GoodKey
{
    public int Value { get; set; }
    
    public override int GetHashCode()
    {
        return Value; // Использует все биты значения
    }
}
```

**Коэффициент загрузки (Load Factor):**
```csharp
// Dictionary автоматически увеличивает размер при load factor > 0.72
// Это обеспечивает баланс между памятью и производительностью
```

## 3. Емкость и ресайзинг

### 3.1. Initial Capacity

**Выбор начальной емкости:**
```csharp
// Если известно примерное количество элементов
var dict = new Dictionary<string, int>(1000); // Избегаем ресайзов
```

**Влияние на производительность:**
```csharp
// Плохо: множественные ресайзы
var dict = new Dictionary<string, int>();
for (int i = 0; i < 10000; i++)
{
    dict.Add(i.ToString(), i); // Множественные ресайзы
}

// Хорошо: одна аллокация
var dict = new Dictionary<string, int>(10000);
for (int i = 0; i < 10000; i++)
{
    dict.Add(i.ToString(), i);
}
```

### 3.2. Процесс ресайза

**Алгоритм ресайза:**
1. Создание нового массива большего размера (обычно в 2 раза больше)
2. Перехеширование всех существующих элементов
3. Замена старых массивов новыми

**Оптимизация:**
```csharp
// Dictionary использует простые числа для размера
// Это улучшает распределение хешей
private static readonly int[] primes = {
    3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919, 1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591, 17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437, 187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263, 1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369
};
```

## 4. Потокобезопасность

### 4.1. Потокобезопасные альтернативы

**ConcurrentDictionary:**
```csharp
var concurrentDict = new ConcurrentDictionary<string, int>();

// Атомарные операции
concurrentDict.TryAdd("key", 1);
concurrentDict.TryUpdate("key", 2, 1);
concurrentDict.GetOrAdd("key", 3);
concurrentDict.AddOrUpdate("key", 1, (k, v) => v + 1);
```

**ReaderWriterLockSlim:**
```csharp
var dict = new Dictionary<string, int>();
var lockObj = new ReaderWriterLockSlim();

// Чтение (множественные потоки)
lockObj.EnterReadLock();
try
{
    if (dict.TryGetValue("key", out var value))
    {
        // Использование value
    }
}
finally
{
    lockObj.ExitReadLock();
}

// Запись (эксклюзивный доступ)
lockObj.EnterWriteLock();
try
{
    dict["key"] = 42;
}
finally
{
    lockObj.ExitWriteLock();
}
```

### 4.2. ImmutableDictionary

**Неизменяемый словарь:**
```csharp
var dict1 = ImmutableDictionary.Create<string, int>();
var dict2 = dict1.Add("key1", 1);
var dict3 = dict2.Add("key2", 2);

// dict1, dict2, dict3 - разные объекты
// dict1 не содержит элементов
// dict2 содержит {"key1": 1}
// dict3 содержит {"key1": 1, "key2": 2}
```

## 5. Особенности использования

### 5.1. Ключи в Dictionary

**Требования к ключам:**
1. Реализация `IEquatable<T>` для эффективного сравнения
2. Корректная реализация `GetHashCode()`
3. Неизменяемость после добавления в словарь

**Пример правильного ключа:**
```csharp
public sealed class PersonKey : IEquatable<PersonKey>
{
    public string FirstName { get; }
    public string LastName { get; }
    public DateTime BirthDate { get; }
    
    public PersonKey(string firstName, string lastName, DateTime birthDate)
    {
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
    }
    
    public bool Equals(PersonKey other)
    {
        if (ReferenceEquals(other, null)) return false;
        if (ReferenceEquals(this, other)) return true;
        return string.Equals(FirstName, other.FirstName) &&
               string.Equals(LastName, other.LastName) &&
               BirthDate.Equals(other.BirthDate);
    }
    
    public override bool Equals(object obj) => Equals(obj as PersonKey);
    
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + (FirstName?.GetHashCode() ?? 0);
            hash = hash * 23 + (LastName?.GetHashCode() ?? 0);
            hash = hash * 23 + BirthDate.GetHashCode();
            return hash;
        }
    }
}
```

### 5.2. Value Types как ключи

**Особенности:**
```csharp
// Структуры как ключи требуют особого внимания
public struct Point
{
    public int X { get; set; }
    public int Y { get; set; }
    
    public override int GetHashCode()
    {
        return X ^ Y; // Простая, но не всегда эффективная хеш-функция
    }
}

var dict = new Dictionary<Point, string>();
dict.Add(new Point { X = 1, Y = 2 }, "Point (1,2)");

// Внимание: boxing/unboxing при использовании как ключа
```

## 6. Альтернативные реализации

### 6.1. SortedDictionary

**Отличия от Dictionary:**
```csharp
var sortedDict = new SortedDictionary<string, int>();
sortedDict.Add("zebra", 1);
sortedDict.Add("apple", 2);
sortedDict.Add("banana", 3);

// Элементы отсортированы по ключу
// apple, banana, zebra

// Производительность:
// Add/Remove/Get: O(log n) вместо O(1)
// Но поддерживается порядок элементов
```

### 6.2. Lookup

**OneToMany отношения:**
```csharp
var groups = people.ToLookup(p => p.Department);

// Множественные значения на один ключ
foreach (var group in groups)
{
    Console.WriteLine($"Department: {group.Key}");
    foreach (var person in group)
    {
        Console.WriteLine($"  {person.Name}");
    }
}
```

## 7. Производительность и оптимизация

### 7.1. Бенчмарки

**Сравнение производительности:**
```csharp
[Benchmark]
public int DictionaryLookup(Dictionary<string, int> dict, string key)
{
    return dict.TryGetValue(key, out var value) ? value : -1;
}

[Benchmark]
public int ListLookup(List<KeyValuePair<string, int>> list, string key)
{
    return list.FirstOrDefault(kv => kv.Key == key).Value;
}

// Результаты (примерные):
// Dictionary: ~50ns
// List: ~5000ns (в 100 раз медленнее)
```

### 7.2. Memory Footprint

**Потребление памяти:**
```csharp
// Память на элемент (примерно):
// Dictionary<string, int>: ~48 байт на запись
// 包括 хеш-код, указатель на следующий, ключ, значение

// Оптимизация:
// Использование struct ключей и значений для уменьшения аллокаций
```

## 8. Расширенные сценарии

### 8.1. Custom Comparer

**Использование собственного компаратора:**
```csharp
var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
dict.Add("Key", 1);
dict.TryGetValue("key", out var value); // Найдет, несмотря на регистр

// Кастомный компарер
public class CaseInsensitiveComparer : IEqualityComparer<string>
{
    public bool Equals(string x, string y) => 
        string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
    
    public int GetHashCode(string obj) => 
        obj?.ToUpperInvariant().GetHashCode() ?? 0;
}
```

### 8.2. Serialization

**JSON Serialization:**
```csharp
public class MyData
{
    public Dictionary<string, object> Properties { get; set; }
}

var json = JsonSerializer.Serialize(myData);
// Dictionary сериализуется в JSON объект
```

## 9. Диагностика и отладка

### 9.1. Анализ производительности

**Использование профайлера:**
```csharp
// Поиск "горячих точек" в Dictionary
// Анализ количества коллизий
// Мониторинг ресайзов
```

### 9.2. Common Pitfalls

**Типичные ошибки:**
```csharp
// 1. Изменение ключа после добавления
var mutableKey = new MutableKey { Id = 1 };
dict.Add(mutableKey, "value");
mutableKey.Id = 2; // Теперь ключ не найдется!

// 2. Плохая хеш-функция
public override int GetHashCode() => 42; // Все элементы в одном бакете!

// 3. Не потокобезопасный доступ
Parallel.ForEach(items, item =>
{
    dict[item.Key] = item.Value; // Race condition!
});
```

## 10. Best Practices

### 10.1. Рекомендации по использованию

1. **Выбирайте правильную начальную емкость**
   - Избегайте множественных ресайзов

2. **Реализуйте правильные ключи**
   - Неизменяемые, с хорошей хеш-функцией

3. **Используйте потокобезопасные альтернативы**
   - ConcurrentDictionary для многопоточного доступа

4. **Избегайте изменения ключей**
   - Это нарушает внутреннюю структуру словаря

5. **Рассмотрите альтернативы**
   - SortedDictionary если важен порядок
   - Lookup для OneToMany отношений

## 11. Заключение

`Dictionary<TKey, TValue>` является фундаментальной структурой данных в .NET, обеспечивая O(1) доступ к элементам в среднем случае. Понимание внутренней структуры, факторов производительности и правильных паттернов использования позволяет создавать эффективные и масштабируемые приложения.

Выбор правильной реализации словаря (Dictionary, ConcurrentDictionary, SortedDictionary) и правильное использование ключей критически важны для достижения оптимальной производительности.

## Источники

1. Разработка бэкенда: план обучения C#