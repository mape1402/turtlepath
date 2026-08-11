using DataScorpio.Profiles;
using DataScorpio.Testing;
using Heroes.Service.Business.Skills.Models.Requests;
using Heroes.Service.Business.Skills.Validators;
using Heroes.Service.Domain;
using Heroes.Service.Domain.Enums;
using Heroes.Service.Persistence;
using Heroes.Service.Tests.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pelican.Mediator;
using TurtlePath.Domain.Identifier;
using TurtlePath.Testing.EntityFrameworkCore;

namespace Heroes.Service.Tests;

/// <summary>
/// Demonstrates unit and integration tests for the Skill feature.
/// </summary>
public sealed class SkillsEntityTests
{
    /// <summary>
    /// Shows a unit test for skill request validation.
    /// </summary>
    [Fact]
    public void Create_hero_skill_validator_rejects_missing_owner_and_invalid_mastery()
    {
        var validator = new CreateHeroSkillRequestValidator();

        var result = validator.Validate(new CreateHeroSkillRequest(default, "", 101));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateHeroSkillRequest.HeroId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateHeroSkillRequest.Name));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateHeroSkillRequest.Mastery));
    }

    /// <summary>
    /// Shows repeated create automations for the same entity with different request models.
    /// </summary>
    [Fact]
    public async Task Skill_automations_set_owner_alignment_for_hero_and_villain_skills()
    {
        await using var host = await HeroesBusinessTestHost.CreateAsync();
        await using var scope = host.CreateScope();
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<AppDbContext>();
        var mediator = services.GetRequiredService<IMediator>();
        var hero = await dbContext.Set<Hero>().SingleAsync(item => item.Alias == "Solar Sentinel");
        var villain = await dbContext.Set<Villain>().SingleAsync(item => item.Alias == "Cipher Queen");

        var heroSkill = await mediator.Send(new CreateHeroSkillRequest(hero.Id, "Precision rescue", 89), CancellationToken.None);
        var villainSkill = await mediator.Send(new CreateVillainSkillRequest(villain.Id, "Grid sabotage", 96), CancellationToken.None);

        Assert.Equal(Alignment.Hero, heroSkill.OwnerAlignment);
        Assert.Equal(hero.Id, heroSkill.HeroId);
        Assert.Equal(Alignment.Villain, villainSkill.OwnerAlignment);
        Assert.Equal(villain.Id, villainSkill.VillainId);
    }

    /// <summary>
    /// Shows DataScorpio testing with the TurtlePath integration test host.
    /// </summary>
    [Fact]
    public async Task Skill_data_scorpio_testing_filters_and_sorts_real_entities()
    {
        await using var host = await TemplateTestHost
            .CreateIntegrationHost<AppDbContext>(profiles => profiles.AddProfile<SkillTestingQueryProfile>())
            .BuildAsync();
        await host.CreateSchemaAsync<AppDbContext>();

        var dataScorpio = host.Resolve<IDataScorpioTesting<Skill>>();

        await dataScorpio.SeedAsync(
        [
            new Skill { Id = CId.From(Ulid.NewUlid()), Name = "Solar flare", Mastery = 94, OwnerAlignment = Alignment.Hero },
            new Skill { Id = CId.From(Ulid.NewUlid()), Name = "Shadow tracking", Mastery = 81, OwnerAlignment = Alignment.Hero },
            new Skill { Id = CId.From(Ulid.NewUlid()), Name = "Signal hijack", Mastery = 97, OwnerAlignment = Alignment.Villain }
        ]);

        var result = await dataScorpio.ApplyAsync(filters: "alignment==Hero", sorts: "-mastery");

        Assert.True(result.IsSuccess);
        Assert.Equal(["Solar flare", "Shadow tracking"], result.Result.Items.Select(skill => skill.Name));
    }

    private sealed class SkillTestingQueryProfile : QueryProfile<Skill>
    {
        public override void Configure(IQueryProfileBuilder<Skill> builder)
        {
            builder
                .AllowFilter("alignment", skill => skill.OwnerAlignment)
                .AllowSort("mastery", skill => skill.Mastery);
        }
    }
}
