namespace InventoryPlatform.Shared.Results;

public sealed record Error(
    string Code,
    string Message)
{
    public static readonly Error None =
        new(string.Empty, string.Empty);

    public static readonly Error NotFound =
    new("Common.NotFound", "Resource not found.");

    public static readonly Error Validation =
        new("Common.Validation", "Validation failed.");

    public static readonly Error Conflict =
        new("Common.Conflict", "Resource already exists.");
}