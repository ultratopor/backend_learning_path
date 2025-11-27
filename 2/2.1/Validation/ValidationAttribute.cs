using System;

namespace _2._1.Validation
{
    public abstract class ValidationAttribute:Attribute
    {
        public abstract bool IsValid(object? value);
    }

    public class LengthAttribute(int minLength, int maxLength) : ValidationAttribute
    {
        public int Min => minLength;
        public int Max => maxLength;
        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                return true;
            }

            if (value is string str)
            {
                return str.Length >= Min && str.Length <= Max;
            }
            return true;
        }
    }

    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Field)]
    public class EmailAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                return true;
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

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class GenerateValidationAttribute : Attribute
    {
    }
}
