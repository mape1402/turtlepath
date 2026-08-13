using Heroes.Service.Domain;
using Heroes.Service.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using TurtlePath.Domain.Identifier;
using TurtlePath.EntityFrameworkCore;
using IncidentEntity = Heroes.Service.Domain.Incident;

namespace Heroes.Service.Business.Jobs.Services.Universe;

/// <summary>
/// EF-backed seeder used by the one-shot job; the job itself remains persistence-agnostic.
/// </summary>
public sealed class HeroesUniverseSeeder : IHeroesUniverseSeeder
{
    private readonly IDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="HeroesUniverseSeeder"/> class.
    /// </summary>
    /// <param name="dbContext">The application database context abstraction.</param>
    public HeroesUniverseSeeder(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<bool> SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _dbContext.Set<Team>().AnyAsync(cancellationToken))
            return false;

        var justiceLeague = new Team { Id = CreateId(), Name = "Justice League", City = "Metropolis", Headquarters = "Watchtower", Reputation = 95 };
        var rogues = new Team { Id = CreateId(), Name = "Rogues Gallery", City = "Gotham", Headquarters = "Blackgate Underground", Reputation = -40 };

        var sentinel = new Hero { Id = CreateId(), Alias = "Solar Sentinel", RealName = "Elena Ray", City = "Metropolis", PowerLevel = 92, TeamId = justiceLeague.Id, Active = true };
        var nightWolf = new Hero { Id = CreateId(), Alias = "Night Wolf", RealName = "Marcus Vale", City = "Gotham", PowerLevel = 74, TeamId = justiceLeague.Id, Active = true };
        var cipher = new Villain { Id = CreateId(), Alias = "Cipher Queen", RealName = "Ada Voss", Lair = "Mirror Grid", PowerLevel = 88, ThreatLevel = ThreatLevel.Critical, TeamId = rogues.Id };

        _dbContext.AddRange(justiceLeague, rogues, sentinel, nightWolf, cipher);
        _dbContext.AddRange(
            new Skill { Id = CreateId(), HeroId = sentinel.Id, Name = "Solar flare", Mastery = 94, OwnerAlignment = Alignment.Hero },
            new Skill { Id = CreateId(), HeroId = nightWolf.Id, Name = "Shadow tracking", Mastery = 81, OwnerAlignment = Alignment.Hero },
            new Skill { Id = CreateId(), VillainId = cipher.Id, Name = "Signal hijack", Mastery = 97, OwnerAlignment = Alignment.Villain },
            new IncidentEntity { Id = CreateId(), Title = "City grid blackout", City = "Metropolis", ThreatLevel = ThreatLevel.High, SuspectedVillainId = cipher.Id });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static CId CreateId() => CId.From(Ulid.NewUlid());
}
