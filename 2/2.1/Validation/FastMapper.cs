using System.Linq.Expressions;
using System.Reflection;

namespace _2._1.Validation
{

    public static class FastMapper<T> where T : class, new()
    {
        private static readonly Lazy<Func<T, T>> _cloner = new(CreateCloner);

        public static T Clone(T source)
        {
            return _cloner.Value(source);
        }

        private static Func<T, T> CreateCloner()
        {
            var sourceParam = Expression.Parameter(typeof(T), "source");

            var newExpression = Expression.New(typeof(T));

            var memberBindings = new List<MemberBinding>();

            foreach (var prop in typeof(T).GetProperties(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.GetProperty |
                BindingFlags.SetProperty))
            {
                if (prop.GetIndexParameters().Length > 0 || prop.SetMethod == null)
                {
                    continue;
                }

                var sourceProperty = Expression.Property(sourceParam, prop);

                var bindExpression = Expression.Bind(prop, sourceProperty);
                memberBindings.Add(bindExpression);
            }

            var memberInitExpression = Expression.MemberInit(newExpression, memberBindings);

            var lambda = Expression.Lambda<Func<T, T>>(memberInitExpression, sourceParam);

            return lambda.Compile();
        }
    }
}