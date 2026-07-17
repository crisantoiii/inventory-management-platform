namespace InventoryPlatform.Shared.Guards;

public static class Guard
{
    public static void AgainstNullOrWhiteSpace(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                $"{parameterName} cannot be null or whitespace.",
                parameterName);
        }
    }

    public static void AgainstNegative(decimal value, string parameterName)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                $"{parameterName} cannot be negative.");
        }
    }
}