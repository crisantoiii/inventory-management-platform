namespace InventoryPlatform.Domain.Entities;

public readonly record struct CustomerId
{
    public Guid Value { get; }

    private CustomerId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("CustomerId cannot be empty.", nameof(value));
        }
        Value = value;
    }

    // High-performance sequential UUIDv7 creation
    public static CustomerId New() => new(Guid.CreateVersion7());

    // Used to reconstruct the type from raw strings or database Guids
    public static CustomerId From(Guid value) => new(value);
    public static CustomerId From(string value) => new(Guid.Parse(value));

    public override string ToString() => Value.ToString();
}