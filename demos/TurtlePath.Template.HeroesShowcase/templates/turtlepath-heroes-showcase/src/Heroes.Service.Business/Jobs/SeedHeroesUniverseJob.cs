using Heroes.Service.Business.Jobs.Services.Universe;
using Heroes.Service.Business.Services.Audit;
using TurtlePath.Jobs;

namespace Heroes.Service.Business.Jobs;

/// <summary>
/// One-shot job that seeds enough data to explore the generated API without hand crafting payloads.
/// </summary>
public sealed class SeedHeroesUniverseJob : TurtlePathJob
{
    private readonly IHeroesUniverseSeeder _heroesUniverseSeeder;
    private readonly IAuditTrail _auditTrail;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeedHeroesUniverseJob"/> class.
    /// </summary>
    public SeedHeroesUniverseJob(IHeroesUniverseSeeder heroesUniverseSeeder, IAuditTrail auditTrail)
    {
        _heroesUniverseSeeder = heroesUniverseSeeder;
        _auditTrail = auditTrail;
    }

    /// <inheritdoc />
    public override async Task ExecuteAsync(TurtlePathJobContext context, CancellationToken cancellationToken)
    {
        if (await _heroesUniverseSeeder.SeedAsync(cancellationToken))
            _auditTrail.Add("Seeded heroes universe demo data.");
    }
}
