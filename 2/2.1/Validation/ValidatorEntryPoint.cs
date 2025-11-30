using _2._1.Data;

namespace _2._1.Validation;

public class ValidatorEntryPoint
{
    public static List<string> Validate(object obj)
    {
        return obj switch
        {
            UserDto user => UserValidator<UserDto>.Validate(user),
            ProductDto product => ProductValidator<ProductDto>.Validate(product),
            OrderDto order => OrderValidator<OrderDto>.Validate(order),
            _ => ["Unknown type."]
        };
    }
}