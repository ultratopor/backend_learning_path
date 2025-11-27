using _2._1.Data;

namespace _2._1.Validation;

public class UserDtoValidatorStatic
{
    // Этот метод не должен содержать рефлексии.
    public static List<string> Validate(UserDto obj)
    {
        var errors = new List<string>();

        // 1. Проверка Name (String Length)
        if (obj.Name == null || obj.Name.Length < 5 || obj.Name.Length > 50) 
        { 
            errors.Add("Name must be between 5 and 50 characters long.");
        }

        // 2. Проверка ContactEmail (Email Format)
        if (!IsValid(obj.ContactEmail)) 
        { 
            errors.Add("ContactEmail is not in a valid email format.");
        }
        
        
        // 3. Проверка Age (Integer Range)
        if (obj.Age < 18 || obj.Age > 120) 
        { 
            errors.Add("Age must be between 18 and 120.");
        }

        return errors;
    }

    private static bool IsValid(string value)
    {
        if (value == null)
        {
            return false;
        }
        if (value is string str)
        {
            var trimmedEmail = str.Trim();
            if (trimmedEmail.EndsWith(".")) return false;
            var atIndex = trimmedEmail.LastIndexOf('@');
            return atIndex > 0 && atIndex < trimmedEmail.Length - 1 &&
                   trimmedEmail.Contains('.', (StringComparison)atIndex);
        }
        return true;
    }
}