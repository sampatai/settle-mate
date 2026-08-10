namespace SettleMate.Abstractions;


public abstract class AuditableEntity 
{
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    protected AuditableEntity(DateTime createUtc, DateTime updateUtc)
    {
        CreatedAtUtc = createUtc;
        UpdatedAtUtc = updateUtc;
    }
    public void UpdateTimestamps(DateTime updateUtc)
    {
        UpdatedAtUtc = updateUtc;
    }
    public void CreateTimestamps(DateTime createUtc, DateTime updateUtc)
    {
        CreatedAtUtc = createUtc;
        UpdatedAtUtc = updateUtc;
    }
}