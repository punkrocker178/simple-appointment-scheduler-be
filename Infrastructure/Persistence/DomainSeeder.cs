using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

public static class DomainSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext db,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var skillsAdded = await SeedSkillsAsync(db, cancellationToken);

        var dealershipAdded = false;
        if (!await db.Dealerships.AnyAsync(d => d.Id == DomainSeedData.DealershipIds.Main, cancellationToken))
        {
            db.Dealerships.Add(DomainSeedData.Dealership);
            dealershipAdded = true;
        }

        var serviceTypesAdded = await SeedServiceTypesAsync(db, cancellationToken);
        var serviceBaysAdded = await SeedServiceBaysAsync(db, cancellationToken);

        if (skillsAdded == 0 && !dealershipAdded && serviceTypesAdded == 0 && serviceBaysAdded == 0)
        {
            logger.LogInformation("Domain seed skipped. All seed records already exist.");
            return;
        }

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Domain seed applied. Added dealership: {DealershipAdded}, skills: {SkillsAdded}, service types: {ServiceTypesAdded}, service bays: {ServiceBaysAdded}.",
            dealershipAdded,
            skillsAdded,
            serviceTypesAdded,
            serviceBaysAdded);
    }

    private static async Task<int> SeedSkillsAsync(ApplicationDbContext db, CancellationToken cancellationToken)
    {
        var seedIds = DomainSeedData.Skills.Select(s => s.Id).ToList();
        var existingIds = await db.Skills
            .Where(s => seedIds.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        var missing = DomainSeedData.Skills.Where(s => !existingIds.Contains(s.Id)).ToList();
        db.Skills.AddRange(missing);
        return missing.Count;
    }

    private static async Task<int> SeedServiceTypesAsync(ApplicationDbContext db, CancellationToken cancellationToken)
    {
        var seedIds = DomainSeedData.ServiceTypes.Select(st => st.Id).ToList();
        var existingIds = await db.ServiceTypes
            .Where(st => seedIds.Contains(st.Id))
            .Select(st => st.Id)
            .ToListAsync(cancellationToken);

        var missing = DomainSeedData.ServiceTypes.Where(st => !existingIds.Contains(st.Id)).ToList();
        db.ServiceTypes.AddRange(missing);
        return missing.Count;
    }

    private static async Task<int> SeedServiceBaysAsync(ApplicationDbContext db, CancellationToken cancellationToken)
    {
        var seedIds = DomainSeedData.ServiceBays.Select(sb => sb.Id).ToList();
        var existingIds = await db.ServiceBays
            .Where(sb => seedIds.Contains(sb.Id))
            .Select(sb => sb.Id)
            .ToListAsync(cancellationToken);

        var missing = DomainSeedData.ServiceBays.Where(sb => !existingIds.Contains(sb.Id)).ToList();
        db.ServiceBays.AddRange(missing);
        return missing.Count;
    }
}
