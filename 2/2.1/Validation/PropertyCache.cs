using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace _2._1.Validation
{
    public static class PropertyCache<T, TProperty>
    {
        private static readonly ConcurrentDictionary<(T, string), Func<T, TProperty>> _propertyCache = new();

        public static Func<T, TProperty> CreateGetter(string propertyName)
        {
            var param = Expression.Parameter(typeof(T), "source");
            var property = Expression.Property(param, propertyName);
            var lambda = Expression.Lambda<Func<T, TProperty>>(property, param);
            _propertyCache.TryAdd((param, propertyName), lambda.Compile());

            return lambda.Compile();

        }
    }
}
