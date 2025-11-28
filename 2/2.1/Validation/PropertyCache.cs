using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace _2._1.Validation
{
    public static class PropertyCache
    {
        //private static readonly ConcurrentDictionary<(T, string), Func<T, TProperty>> _propertyCache = new();

        public static Action<T, TProperty> CreateConditionalSetter<T, TProperty>(string propertyName)
        {
            var propertyInfo = typeof(T).GetProperty(propertyName, typeof(TProperty));
            if(propertyInfo == null || propertyInfo.CanWrite)
            {
                throw new InvalidOperationException(
                    $"Свойство '{propertyName}' доступное для записи не найдено в типе '{typeof(T).Name}'.");
            }

            var source = Expression.Parameter(typeof(T), "source");
            var value = Expression.Parameter(typeof(TProperty), "value");

            var nullConstant = Expression.Constant(null, typeof(TProperty));
            var notNullTest = Expression.NotEqual(value, nullConstant);

            var propertyAccess = Expression.Property(source, propertyInfo);
            var assignment = Expression.Assign(propertyAccess, value);

            var doNothing = Expression.Empty();

            var condition = Expression.Condition(notNullTest, assignment, doNothing);

            var lambda = Expression.Lambda<Action<T, TProperty>>(condition, source, value);

            return lambda.Compile();
        }
    }

    public static class DynamicDelegateCache
    {
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, Delegate>> _cache = new();

        public static Action<T, TProperty> GetSetter<T, TProperty>(string propertyName)
        {
            var typeCache = _cache.GetOrAdd(typeof(T), _ => new ConcurrentDictionary<string, Delegate>());

            var setterDelegate = typeCache.GetOrAdd(propertyName, _ =>
            {
                return PropertyCache.CreateConditionalSetter<T, TProperty>(propertyName);
            });

            return (Action<T, TProperty>)setterDelegate;
        }
    }
}
