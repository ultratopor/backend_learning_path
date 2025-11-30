using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace _2._1.Validation
{
    public class Validator
    {
        public List<string> Validate<T>(T? obj) where T : class
        {
            var errors = new List<string>();
            if (obj == null)
            {
                errors.Add("Object to validate is null.");
                return errors;
            }
            var type = typeof(T); // нет упаковки
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(obj); // упаковка
                // проверка атрибута длины
                var lengthAttribute = property.GetCustomAttribute<LengthAttribute>();
                if (lengthAttribute != null)
                {
                    if (value is string stringValue)
                    {
                        if (stringValue.Length < lengthAttribute.Min || stringValue.Length > lengthAttribute.Max)
                        {
                            errors.Add($"Property {property.Name} length is out of range ({lengthAttribute.Min}-{lengthAttribute.Max}).");
                        }
                    }
                    else
                    {
                        errors.Add($"Property {property.Name} is not a string.");
                    }
                }
                // проверка атрибута почты
                var emailAttribute = property.GetCustomAttribute<EmailAttribute>();
                if (emailAttribute != null)
                {
                    if (value is string emailValue)
                    {
                        if (!emailAttribute.IsValid(emailValue))
                        {
                            errors.Add($"Property {property.Name} is not a valid email.");
                        }
                    }
                    else
                    {
                        errors.Add($"Property {property.Name} is not a string.");
                    }
                }

            }
            return errors;
        }
        
        public List<string> Validate(object obj)
        {
            var errors = new List<string>();
            if(obj == null)
            {
                errors.Add("Object to validate is null.");
                return errors;
            }
            var type = obj.GetType();
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(obj);
                // проверка атрибута длины
                var lengthAttribute = property.GetCustomAttribute<LengthAttribute>();
                if(lengthAttribute != null)
                {
                    if(value is string stringValue)
                    {
                        if(stringValue.Length < lengthAttribute.Min || stringValue.Length > lengthAttribute.Max)
                        {
                            errors.Add($"Property {property.Name} length is out of range ({lengthAttribute.Min}-{lengthAttribute.Max}).");
                        }
                    }
                    else
                    {
                        errors.Add($"Property {property.Name} is not a string.");
                    }
                }

                // проверка атрибута почты
                var emailAttribute = property.GetCustomAttribute<EmailAttribute>();
                if (emailAttribute != null)
                {
                    if(value is string emailValue)
                    {
                        if(!emailAttribute.IsValid(emailValue))
                        {
                            errors.Add($"Property {property.Name} is not a valid email.");
                        }
                    }
                    else
                    {
                        errors.Add($"Property {property.Name} is not a string.");
                    }
                }
            }
            return errors;
        }
    }

    public class AdvancedValidator
    {
        private static readonly ConcurrentDictionary<Type, Func<object, List<string>>> _cache = new ();
        public List<string> Validate(object obj)
        {
            if(obj == null)
            {
                return ["Object to validate is null."];
            }

            var validator = _cache.GetOrAdd(obj.GetType(), CreateValidator);
            return validator(obj);
        }

        private Func<object, List<string>> CreateValidator(Type type)
        {
            // создаем объект
            var param = Expression.Parameter(typeof(object), "obj");
            // конвертируем в нужный тип
            var typeObj = Expression.Convert(param, type);

            // создаем список ошибок
            var errorsVar = Expression.Variable(typeof(List<string>), "errors");
            var newErrorList = Expression.New(typeof(List<string>));

            // еррорсы
            var blockExpressions = new List<Expression>
            {
                Expression.Assign(errorsVar, newErrorList)
            };

            //ищем свойства
            foreach (PropertyInfo property in type.GetProperties())
            {
                var propertyValue = Expression.Property(typeObj, property);

                // LengthAttribute
                var lengthAttribute = property.GetCustomAttribute<LengthAttribute>();
                if(lengthAttribute != null)
                {
                    var ifElseLabel = Expression.Label();
                    // Условие: stringValue.Length >= Min && stringValue.Length <= Max
                    var lengthCheck = Expression.AndAlso(
                        Expression.GreaterThanOrEqual(
                            Expression.Property(propertyValue, "Length"),
                            Expression.Constant(lengthAttribute.Min)
                            ),
                        Expression.LessThanOrEqual(
                            Expression.Property(propertyValue, "Length"),
                            Expression.Constant(lengthAttribute.Max)
                            )
                        );

                    // если условие не выполнено, добавляем ошибку
                    var addError = Expression.Call(
                        errorsVar,
                        "Add", null,
                        Expression.Constant($"Property {property.Name} length is out of range ({lengthAttribute.Min}-{lengthAttribute.Max}).")
                        );

                    // блок if
                    var ifBlock = Expression.IfThen(
                        Expression.Not(lengthCheck),    // if (!lengthCheck)
                        addError                        // { errors.Add(...); }
                        );
                    blockExpressions.Add(ifBlock);
                }

                // EmailAttribute
                var emailAttribute = property.GetCustomAttribute<EmailAttribute>();
                if(emailAttribute != null)
                {
                    // вызов метода IsValid
                    var isValidCall = typeof(EmailAttribute).GetMethod("IsValid", BindingFlags.Public|
                        BindingFlags.Static);
                    
                    // блок if
                    var ifBlock = Expression.IfThen(
                        Expression.Not(Expression.Call(isValidCall, propertyValue)),    // if (!isValidCall)
                        Expression.Call(errorsVar, "Add", null,                      // { errors.Add(...); }
                        Expression.Constant($"Property {property.Name} is not a valid email."))
                        );
                    blockExpressions.Add(ifBlock);
                }
            }
            blockExpressions.Add(errorsVar); // возвращаем errors
            // сборка
            var body = Expression.Block([errorsVar], blockExpressions);

            return Expression.Lambda<Func<object, List<string>>>(body, param).Compile();
        }
    }
}
