using DataScorpio.Profiles;
using Heroes.Service.Business;
using Heroes.Service.Domain.Identifier;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TurtlePath.Domain.Identifier;
using TurtlePath.EntityFrameworkCore;
using TurtlePath.Testing;
using TurtlePath.Testing.EntityFrameworkCore;
using TurtlePath.Testing.Integration;

namespace Heroes.Service.Tests.Testing;

public static class TemplateTestHost
{
    private static readonly Action<QueryProfileRegistryBuilder> _defaultDataScorpioProfiles =
        profiles => profiles.FromAssembly(typeof(Constants).Assembly);

    public static TurtlePathTestHostBuilder CreateUnitHost()
        => TurtlePathTestHost.Create();

    public static TurtlePathTestHostBuilder CreateIntegrationHost<TDbContext>(
        Action<QueryProfileRegistryBuilder> configureDataScorpio = null)
        where TDbContext : DbContext, IDbContext
    {
        var businessAssembly = typeof(Constants).Assembly;

        return TurtlePathTestHost
            .Create()
            .UsePelicanTesting(businessAssembly)
            .UseOctoMapTesting(businessAssembly)
            .UseCrabalidatorTesting(businessAssembly)
            .UseSpiderTesting(businessAssembly)
            .UseDataScorpioTesting(configureDataScorpio ?? _defaultDataScorpioProfiles)
            .UseApplicationServices(services =>
            {
                services
                    .AddTurtlePath(businessAssembly)
                    .UseCId<Ulid, string>(config =>
                    {
                        config.DefaultFactory = () => CId.From(Ulid.NewUlid());
                        config.ConvertToDb = id => id.ToString();
                        config.ConvertFromDb = value => CId.From(Ulid.Parse(value));
                        config.JsonConverter = value => string.IsNullOrEmpty(value) ? CId.From(Ulid.Empty) : CId.From(Ulid.Parse(value));
                        config.NullableJsonConverter = value => string.IsNullOrEmpty(value) ? null : CId.From(Ulid.Parse(value));
                        config.ParseFunction = value => CId.From(Ulid.Parse(value));
                        config.ToByteArrayFunction = value => value.ToByteArray();
                    })
                    .UseCIdProfiles(typeof(HeroesIdentifierProfile).Assembly);
            })
            .UseSqliteDbContext<TDbContext>(options => options with
            {
                ConfigurationAssemblies = [typeof(TDbContext).Assembly]
            });
    }
}
