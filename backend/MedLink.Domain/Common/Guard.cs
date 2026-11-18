namespace MedLink.Domain.Common
{
     
    internal static class Guard
    {
        public static string RequireText(string? value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value is required.", paramName);

            return value.Trim();
        }

        
        public static string? OptionalText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        public static int RequireId(int value, string paramName)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(paramName, "Id must be greater than zero.");

            return value;
        }

        public static int RequirePositive(int value, string paramName)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(paramName, "Value must be greater than zero.");

            return value;
        }

        public static decimal RequireNonNegative(decimal value, string paramName)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(paramName, "Value cannot be negative.");

            return value;
        }
    }
}
