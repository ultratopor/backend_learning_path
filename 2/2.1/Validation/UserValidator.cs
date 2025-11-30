namespace _2._1.Validation;

public class UserValidator<T> where T : class
{
    public static List<string> Validate(T? obj)
    {
        var errors = new List<string>();
        if (obj == null)
        {
            errors.Add("Object to validate is null.");
            return errors;
        }
        var type = typeof(T); // нет упаковки
        // ...какие-то действия
        return errors;
    }
}