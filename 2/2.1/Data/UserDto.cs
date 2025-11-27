using _2._1.Validation;
using System.ComponentModel.DataAnnotations;

namespace _2._1.Data;

public class UserDto
{
    // Длина: от 5 до 50 символов
    [_2._1.Validation.Length(5, 50)]
    public string Name { get; set; }

    // Проверка Email 
    [Email]
    public string ContactEmail { get; set; }

    // Числовое ограничение: от 18 до 120
    [Range(18, 120)]
    public int Age { get; set; }
}