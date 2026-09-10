namespace SettleMate.Abstractions;

/// <summary>
/// Represents a unit type for handlers that don't require input parameters.
/// </summary>
public readonly record struct Unit
{
    public static Unit Value => default;
}
