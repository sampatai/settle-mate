using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SettleMate.Authorization;
using SettleMate.Database.Entities.Identity;
using SettleMate.Database.Entities.Onboarding;
using SettleMate.Database.Entities.Accommodation;
using System.Text.Json;

namespace SettleMate.Database;

public static class DatabaseSeedService
{
    public static async Task SeedAsync(
        ApplicationDbContext dbContext,
        UserManager<User> userManager,
        RoleManager<Role> roleManager)
    {
        await dbContext.Database.MigrateAsync();
        await SeedOnboardingReferenceDataAsync(dbContext);
        await SeedAccommodationDataAsync(dbContext);
        var rolePermissions = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["User"] = [Permissions.ListingsRead, Permissions.ListingScoresRead, Permissions.SavedListingsRead, Permissions.SavedListingsCreate, Permissions.SavedListingsDelete, Permissions.DestinationsRead, Permissions.DestinationsCreate, Permissions.DestinationsUpdate, Permissions.DestinationsDelete],
            ["ContentCreator"] = [Permissions.ContentRead, Permissions.ContentCreate, Permissions.ContentUpdate, Permissions.ContentDelete],
            ["ListingProvider"] = [Permissions.ListingsRead, Permissions.ListingsCreate, Permissions.ListingsUpdate, Permissions.ListingsDelete, Permissions.ListingScoresRead, Permissions.ListingScoresUpsert],
            ["Admin"] = Permissions.All
        };
        foreach (var (roleName, permissions) in rolePermissions)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                var roleResult = await roleManager.CreateAsync(new Role { Name = roleName });
                if (!roleResult.Succeeded)
                    throw new InvalidOperationException($"Unable to create required role '{roleName}': " +
                        string.Join(", ", roleResult.Errors.Select(error => error.Description)));
                role = await roleManager.FindByNameAsync(roleName);
            }
            if (role is not null)
                await EnsureRolePermissionsAsync(roleManager, role, permissions);
        }
    }

    private static async Task SeedOnboardingReferenceDataAsync(ApplicationDbContext dbContext)
    {
        var rules = new[]
        {
            new VisaRule { VisaSubclass = "500", WorkHourLimitPerFortnight = 48, DependentBachelorWorkHourLimitPerFortnight = 48, DependentPostgraduateWorkHourLimitPerFortnight = null, TfnEligible = true, NdisEligible = false, BlueCardRequiredForChildRelatedWork = true, RequiredDocumentsJson = JsonSerializer.Serialize(new[] { "Passport", "Evidence of visa grant", "Proof of Australian address" }) },
            new VisaRule { VisaSubclass = "417", WorkHourLimitPerFortnight = null, TfnEligible = true, NdisEligible = false, BlueCardRequiredForChildRelatedWork = true, RequiredDocumentsJson = JsonSerializer.Serialize(new[] { "Passport", "Evidence of visa grant", "Employment records" }) },
            new VisaRule { VisaSubclass = "462", WorkHourLimitPerFortnight = null, TfnEligible = true, NdisEligible = false, BlueCardRequiredForChildRelatedWork = true, RequiredDocumentsJson = JsonSerializer.Serialize(new[] { "Passport", "Evidence of visa grant", "Employment records" }) },
            new VisaRule { VisaSubclass = "485", WorkHourLimitPerFortnight = null, TfnEligible = true, NdisEligible = false, BlueCardRequiredForChildRelatedWork = true, RequiredDocumentsJson = JsonSerializer.Serialize(new[] { "Passport", "Evidence of visa grant", "Qualification evidence" }) },
            new VisaRule { VisaSubclass = "482", WorkHourLimitPerFortnight = null, TfnEligible = true, NdisEligible = false, BlueCardRequiredForChildRelatedWork = true, RequiredDocumentsJson = JsonSerializer.Serialize(new[] { "Passport", "Evidence of visa grant", "Employment contract" }) },
            new VisaRule { VisaSubclass = "600", WorkHourLimitPerFortnight = 0, TfnEligible = false, NdisEligible = false, BlueCardRequiredForChildRelatedWork = true, RequiredDocumentsJson = JsonSerializer.Serialize(new[] { "Passport", "Evidence of visa grant", "Travel and health documents" }) }
        };
        foreach (var rule in rules)
        {
            var existingRule = await dbContext.VisaRules
                .SingleOrDefaultAsync(x => x.VisaSubclass == rule.VisaSubclass);
            if (existingRule is null)
                dbContext.VisaRules.Add(rule);
            else
            {
                existingRule.WorkHourLimitPerFortnight = rule.WorkHourLimitPerFortnight;
                existingRule.DependentBachelorWorkHourLimitPerFortnight = rule.DependentBachelorWorkHourLimitPerFortnight;
                existingRule.DependentPostgraduateWorkHourLimitPerFortnight = rule.DependentPostgraduateWorkHourLimitPerFortnight;
                existingRule.TfnEligible = rule.TfnEligible;
                existingRule.NdisEligible = rule.NdisEligible;
                existingRule.BlueCardRequiredForChildRelatedWork = rule.BlueCardRequiredForChildRelatedWork;
                existingRule.RequiredDocumentsJson = rule.RequiredDocumentsJson;
            }
        }

        var templates = new[]
        {
            new ChecklistTemplate { Key = "arrival:address", WeekNumber = 1, Title = "Confirm your address and emergency contacts", Description = "Save your local address, emergency contact and important visa details in one secure place." },
            new ChecklistTemplate { Key = "arrival:bank-account", WeekNumber = 1, Title = "Open an Australian bank account", Description = "Apply online or visit a bank branch. The bank may request additional identity documents; confirm the list with your chosen bank before attending.", Provider = "Your chosen Australian bank", EligibilityNotes = "Banks set their own identification process and may require an in-person identity check.", RequiredDocumentsJson = JsonSerializer.Serialize(new[] { "Passport", "Visa grant or VEVO evidence", "Australian address", "Mobile number", "Tax residency details" }) },
            new ChecklistTemplate { Key = "arrival:healthcare", WeekNumber = 1, Title = "Understand healthcare access", Description = "Check Medicare, private cover and your visa health requirements." },
            new ChecklistTemplate { Key = "arrival:transport", WeekNumber = 1, Title = "Set up local transport", Description = "Use your state or territory transport authority (for example Transport for NSW, myki/Victoria, Translink/Queensland or SmartRider/WA) to get the local travel card or app. Check the authority's current requirements before applying.", Provider = "Your state or territory transport authority", EligibilityNotes = "Concession fares require evidence of an eligible student, concession or pension status.", RequiredDocumentsJson = JsonSerializer.Serialize(new[] { "Passport or accepted ID", "Concession evidence if applicable", "Payment card", "Australian address if requested" }) },
            new ChecklistTemplate { Key = "career:resume", WeekNumber = 2, Title = "Adapt your resume to the local market", Description = "Create a concise resume and a reusable cover letter for your career goal." },
            new ChecklistTemplate { Key = "career:network", WeekNumber = 4, Title = "Join one professional or community network", Description = "Choose a group connected to your university, employer or career goal." },
            new ChecklistTemplate { Key = "settlement:budget", WeekNumber = 5, Title = "Review your first-month budget", Description = "Compare actual spending with your selected budget range and adjust your plan." },
            new ChecklistTemplate { Key = "settlement:check-in", WeekNumber = 8, Title = "Review your settlement progress", Description = "Check completed tasks, update your goals and choose the next milestone." },
            new ChecklistTemplate { Key = "career:skills", WeekNumber = 12, Title = "Choose a skills or credential next step", Description = "Select one realistic course, accreditation or portfolio milestone." }
            ,            new ChecklistTemplate { Key = "career:aged-care-screening", WeekNumber = 3, Title = "Complete aged-care worker screening", CareerGoal = "Aged Care", Description = "Before accepting aged-care work, contact the employer and relevant screening authority. Do not assume an NDIS clearance alone covers every aged-care role.", Provider = "Employer, aged-care provider and relevant state or territory screening authority", ApplicationUrl = "https://www.health.gov.au/topics/aged-care-workforce/screening-requirements", EligibilityNotes = "A police certificate or approved clearance is required before starting covered aged-care work; providers may also require vaccinations, first aid and qualifications.", RequiredDocumentsJson = JsonSerializer.Serialize(new[] { "Passport or other identity documents", "Visa or VEVO work-right evidence", "Residential and employment history", "Recent photograph", "Police certificate or NDIS Worker Screening clearance", "Qualifications and certificates if requested", "Immunisation evidence if requested" }) }
            ,new ChecklistTemplate { Key = "career:child-care-screening", WeekNumber = 3, Title = "Apply for child-related work screening", CareerGoal = "Child Care", Description = "Ask the employer which screening applies in your state or territory. Queensland uses a Blue Card; other jurisdictions use their own working-with-children scheme. Wait for clearance before starting regulated child-related work.", Provider = "Your state or territory working-with-children screening authority", ApplicationUrl = "https://www.education.gov.au/early-childhood/providers/howto/who-can-administer/background-checks", EligibilityNotes = "Early childhood education and care workers must hold the required clearance before starting; the exact process varies by jurisdiction.", RequiredDocumentsJson = JsonSerializer.Serialize(new[] { "Passport or accepted identity documents", "Visa or VEVO work-right evidence", "Address history", "Recent photograph", "Qualifications or enrolment evidence", "First aid and CPR certificates if requested", "Police check or working-with-children clearance" }) }
            ,new ChecklistTemplate { Key = "career:construction-white-card", WeekNumber = 3, Title = "Obtain construction site safety induction", CareerGoal = "Construction", Description = "Complete CPCCWHS1001 general construction induction (White Card) through an approved registered training organisation and keep evidence. A site-specific induction and extra licences may also be required.", Provider = "Approved registered training organisation and state or territory WHS regulator", ApplicationUrl = "https://www.safeworkaustralia.gov.au/safety-topic/managing-health-and-safety/licences", EligibilityNotes = "White Card training is required before construction work. High-risk work such as forklifts, cranes, dogging or rigging requires the relevant high-risk work licence.", RequiredDocumentsJson = JsonSerializer.Serialize(new[] { "Passport or accepted identity documents", "Visa or VEVO work-right evidence", "White Card training evidence", "Trade or skills certificates if applicable", "High-risk work licence if applicable", "Site-specific induction evidence" }) }
            ,new ChecklistTemplate { Key = "career:aged-care-documents", WeekNumber = 4, Title = "Prepare aged-care employment documents", CareerGoal = "Aged Care", Description = "Collect your resume, passport, visa or VEVO work-right evidence, Australian address and bank details, tax file number, qualifications and certificates, immunisation evidence if requested, police check and screening clearance. Confirm with the provider whether it requires NDIS Worker Screening, a Working with Children Check or a first-aid certificate for the specific role.", EstimatedMinutes = 45, RequiredDocumentsJson = JsonSerializer.Serialize(new[] { "Resume", "Passport", "Visa or VEVO work-right evidence", "Qualifications and certificates", "Tax file number", "Screening clearance" }) }
            ,new ChecklistTemplate { Key = "career:aged-care-ndis-screening-qld", WeekNumber = 3, Title = "Apply for NDIS Worker Screening", CareerGoal = "Aged Care", State = "QLD", Description = "Confirm whether your aged-care role is a risk-assessed NDIS role and apply for the Queensland NDIS Worker Screening Check before starting regulated work.", Provider = "Queensland Government", ApplicationUrl = "https://www.qld.gov.au/disability/work/employment-screening", EligibilityNotes = "The provider must verify your clearance before you perform risk-assessed work with NDIS participants.", EstimatedMinutes = 45, IsTimeSensitive = true, RequiredDocumentsJson = JsonSerializer.Serialize(new[] { "Passport or accepted identity documents", "Visa or VEVO work-right evidence", "Residential address history", "Recent photograph", "Employer or provider details" }) }
            ,new ChecklistTemplate { Key = "career:aged-care-blue-card-qld", WeekNumber = 3, Title = "Check Queensland Blue Card requirements", CareerGoal = "Aged Care", State = "QLD", Description = "Ask your employer whether the role involves child-related work and confirm whether you need a Queensland Blue Card before commencing those duties.", Provider = "Queensland Government Blue Card Services", ApplicationUrl = "https://www.qld.gov.au/law/laws-regulated-industries-and-accountability/queensland-laws-and-regulations/blue-card", EligibilityNotes = "A Blue Card is required for regulated child-related work in Queensland; it is not a substitute for NDIS Worker Screening.", EstimatedMinutes = 30, IsTimeSensitive = true, RequiredDocumentsJson = JsonSerializer.Serialize(new[] { "Passport or accepted identity documents", "Visa or VEVO work-right evidence", "Employer or organisation details", "Address history" }) }
            ,new ChecklistTemplate { Key = "career:child-care-documents", WeekNumber = 4, Title = "Prepare child-care employment documents", CareerGoal = "Child Care", Description = "Collect your resume, passport, visa or VEVO work-right evidence, qualifications or enrolment evidence, first-aid and CPR certificates if requested, immunisation evidence if requested, police check and the relevant state or territory working-with-children clearance. The centre or regulator may request certified copies or referee details." }
            ,new ChecklistTemplate { Key = "career:construction-documents", WeekNumber = 4, Title = "Prepare construction employment documents", CareerGoal = "Construction", Description = "Collect your resume, passport, visa or VEVO work-right evidence, White Card, trade or skills certificates, licences for plant or high-risk work, bank details, tax file number and any site-specific medical or police checks. Ask the builder or labour-hire company for the exact site induction and licence requirements." }
        };
        foreach (var template in templates)
        {
            var existingTemplate = await dbContext.ChecklistTemplates
                .SingleOrDefaultAsync(x => x.Key == template.Key);
            if (existingTemplate is null)
                dbContext.ChecklistTemplates.Add(template);
            else
            {
                existingTemplate.WeekNumber = template.WeekNumber;
                existingTemplate.Title = template.Title;
                existingTemplate.Description = template.Description;
                existingTemplate.VisaSubclass = template.VisaSubclass;
                existingTemplate.State = template.State;
                existingTemplate.CareerGoal = template.CareerGoal;
                existingTemplate.Provider = template.Provider;
                existingTemplate.ApplicationUrl = template.ApplicationUrl;
                existingTemplate.EligibilityNotes = template.EligibilityNotes;
                existingTemplate.EstimatedMinutes = template.EstimatedMinutes;
                existingTemplate.IsTimeSensitive = template.IsTimeSensitive;
                existingTemplate.RequiredDocumentsJson = template.RequiredDocumentsJson;
            }
        }
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedAccommodationDataAsync(ApplicationDbContext dbContext)
    {
        if (await dbContext.Listings.AnyAsync())
            return;

        var listings = new[]
        {
            new Listing(Guid.Parse("10000000-0000-0000-0000-000000000001"), "seed-provider", "Sunnybank furnished room", "12 Station Road", "Sunnybank", -27.5798m, 153.0993m, 260m, 1, "Private room", new DateOnly(2026, 10, 1), "https://example.com/listings/sunnybank") ,
            new Listing(Guid.Parse("10000000-0000-0000-0000-000000000002"), "seed-provider", "West End student share", "8 Boundary Street", "West End", -27.4806m, 153.0060m, 320m, 1, "Private room", new DateOnly(2026, 9, 20), "https://example.com/listings/west-end")
        };
        foreach (var listing in listings)
        {
            listing.SetStatus(ListingStatus.Approved);
            listing.SetActive(true);
            listing.AddAmenity(new ListingAmenity(Guid.NewGuid(), listing.Id, "Wi-Fi"));
            listing.AddAmenity(new ListingAmenity(Guid.NewGuid(), listing.Id, "Laundry"));
            listing.AddWeeklyCost(new ListingWeeklyCost(Guid.NewGuid(), listing.Id, "Utilities", 25m, false));
            dbContext.Listings.Add(listing);
        }
        dbContext.ListingScores.AddRange(
            new ListingScore(Guid.NewGuid(), listings[0].Id, null, 28, 3, 4, 78, 82, "seed-public-data"),
            new ListingScore(Guid.NewGuid(), listings[1].Id, null, 18, 5, 6, 74, 91, "seed-public-data"));
        await dbContext.SaveChangesAsync();
    }

    private static async Task EnsureRolePermissionsAsync(
        RoleManager<Role> roleManager,
        Role role,
        IEnumerable<string> permissions)
    {
        var existingClaims = await roleManager.GetClaimsAsync(role);
        foreach (var permission in permissions)
        {
            if (existingClaims.Any(claim =>
                claim.Type == CustomClaimTypes.Permission && claim.Value == permission))
                continue;

            var result = await roleManager.AddClaimAsync(
                role,
                new System.Security.Claims.Claim(CustomClaimTypes.Permission, permission));
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Unable to grant permission '{permission}' to role '{role.Name}'.");
        }
    }
}
