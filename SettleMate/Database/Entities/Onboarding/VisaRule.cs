namespace SettleMate.Database.Entities.Onboarding;

public sealed class VisaRule
{
    internal VisaRule() { }

    public string VisaSubclass { get; internal set; } = null!;
    public int? WorkHourLimitPerFortnight { get; internal set; }
    public int? DependentBachelorWorkHourLimitPerFortnight { get; internal set; }
    public int? DependentPostgraduateWorkHourLimitPerFortnight { get; internal set; }
    public bool TfnEligible { get; internal set; }
    public bool NdisEligible { get; internal set; }
    public bool BlueCardRequiredForChildRelatedWork { get; internal set; }
    public string RequiredDocumentsJson { get; internal set; } = "[]";

    public int? GetWorkHourLimit(UserProfile profile) =>
        VisaSubclass != "500" ? WorkHourLimitPerFortnight :
        !profile.CourseStarted ? 0 :
        profile.ApplicantType == "Primary" ? WorkHourLimitPerFortnight :
        profile.StudyLevel == "Bachelor"
            ? DependentBachelorWorkHourLimitPerFortnight
            : DependentPostgraduateWorkHourLimitPerFortnight;
}
