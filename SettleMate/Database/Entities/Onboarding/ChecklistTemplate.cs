namespace SettleMate.Database.Entities.Onboarding;

using System.Text.Json;

public sealed class ChecklistTemplate
{
    internal ChecklistTemplate() { }

    public Guid Id { get; internal set; }
    public string Key { get; internal set; } = null!;
    public int WeekNumber { get; internal set; }
    public string Title { get; internal set; } = null!;
    public string Description { get; internal set; } = null!;
    public string? VisaSubclass { get; internal set; }
    public string? State { get; internal set; }
    public string? CareerGoal { get; internal set; }
    public string? Provider { get; internal set; }
    public string? ApplicationUrl { get; internal set; }
    public string? EligibilityNotes { get; internal set; }
    public string RequiredDocumentsJson { get; internal set; } = "[]";

    public IReadOnlyList<string> RequiredDocuments =>
        JsonSerializer.Deserialize<string[]>(RequiredDocumentsJson) ?? [];

    public string GetGuidanceDescription()
    {
        var guidance = Description;
        if (!string.IsNullOrWhiteSpace(Provider))
            guidance += $" Apply through: {Provider}.";
        if (!string.IsNullOrWhiteSpace(ApplicationUrl))
            guidance += $" Official information: {ApplicationUrl}.";
        if (!string.IsNullOrWhiteSpace(EligibilityNotes))
            guidance += $" Eligibility: {EligibilityNotes}";
        if (RequiredDocuments.Count > 0)
            guidance += $" Documents to prepare: {string.Join(", ", RequiredDocuments)}.";
        return guidance;
    }

    public bool AppliesTo(UserProfile profile) =>
        (VisaSubclass is null || VisaSubclass == profile.VisaSubclass) &&
        (State is null || State == profile.State) &&
        (CareerGoal is null || CareerGoal == profile.CareerGoal);
}
