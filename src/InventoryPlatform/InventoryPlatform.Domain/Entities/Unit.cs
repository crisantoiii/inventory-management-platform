using InventoryPlatform.Domain.Common;
using InventoryPlatform.Shared.Guards;

namespace InventoryPlatform.Domain.Entities;

public sealed class Unit : BaseEntity
{
    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Symbol { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    private Unit() { }

    public Unit(
        string code,
        string name,
        string symbol)
    {
        ChangeCode(code);
        Rename(name);
        ChangeSymbol(symbol);
        IsActive = true;
    }

    public void Update(
        string code,
        string name,
        string symbol)
    {
        ChangeCode(code);
        Rename(name);
        ChangeSymbol(symbol);
    }

    public void ChangeCode(string code)
    {
        Guard.AgainstNullOrWhiteSpace(code, nameof(code));

        Code = code;
    }

    public void Rename(string name)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));

        Name = name;
    }

    public void ChangeSymbol(string symbol)
    {
        Guard.AgainstNullOrWhiteSpace(symbol, nameof(symbol));

        Symbol = symbol;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}