using Crabalidator.DependencyInjection;
using DTemplate.Business;
using DTemplate.Persistence;
using OctoMap;
using System.Diagnostics.CodeAnalysis;
using TurtlePath.Crabalidator;
using TurtlePath.Domain.Identifier;
using TurtlePath.Mapping;
using TurtlePath.OctoMap;
using TurtlePath.Validation;

namespace Microsoft.Extensions.DependencyInjection
{
    [ExcludeFromCodeCoverage]
    internal static class ApplicationExtensions
    {
        internal static IServiceCollection AddApplicationDefaults(this IServiceCollection services)
        {
            services.AddPelican(typeof(Constants).Assembly);

            services.AddCrabalidator(typeof(Constants).Assembly);

            services.AddOctoMap(registration =>
            {
                registration.Options.EnableRuntimeImplicitMaps = true;
                registration.Options.DuplicateMapPolicy = DuplicateMapPolicy.Throw;
                registration.AddMaps(typeof(Constants).Assembly);
            });

            services.AddScoped<IMapperAdapter, OctoMapAdapter>();
            services.AddScoped<IValidatorAdapter, CrabalidatorAdapter>();

            services.AddTurtlePath(typeof(Constants).Assembly)
                .UseOctoMap()
                .UseCrabalidator()
                .UseDataScorpio(profiles => profiles.FromAssembly(typeof(Constants).Assembly))
                .UseCId<Ulid, string>(config =>
                {
                    config.DefaultFactory = () => CId.From(Ulid.NewUlid());
                    config.ConvertToDb = id => id.ToString();
                    config.ConvertFromDb = value => CId.From(Ulid.Parse(value));
                    config.JsonConverter = value => string.IsNullOrEmpty(value) ? CId.From(Ulid.Empty) : CId.From(Ulid.Parse(value));
                    config.NullableJsonConverter = value => string.IsNullOrEmpty(value) ? null : CId.From(Ulid.Parse(value));
                    config.ParseFunction = value => CId.From(Ulid.Parse(value));
                })
                .UseEntityFrameworkCore<AppDbContext>();

            return services;
        }
    }
}
