using System.Linq.Expressions;

namespace _2._1.Validation
{
    public class FastPropertyAccessor<T, TProperty>
    {
        public Func<T, TProperty> Getter;

        public static Func<T, TProperty> CreateGetter(string propertyName)
        {
            var param = Expression.Parameter(typeof(T), "source");
            var property = Expression.Property(param, propertyName);
            var lambda = Expression.Lambda<Func<T, TProperty>>(property, param);
            
            return lambda.Compile();
        }
    }
}
