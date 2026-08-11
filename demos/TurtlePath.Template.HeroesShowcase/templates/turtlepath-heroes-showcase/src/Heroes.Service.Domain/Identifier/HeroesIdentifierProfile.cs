using TurtlePath.Domain.Identifier;

namespace Heroes.Service.Domain.Identifier;

/// <summary>
/// Configures entity-specific CId behavior for legacy entities.
/// </summary>
public sealed class HeroesIdentifierProfile : CIdProfile
{
    /// <summary>
    /// Adds the legacy integer key configuration used by <see cref="LegacyCaseFile"/>.
    /// </summary>
    public override void Configure(CIdProfileBuilder builder)
    {
        builder.UseCIdFor<LegacyCaseFile, int, int>(config =>
        {
            // Legacy rows normally arrive with an id assigned by the old system or by the database.
            config.DefaultFactory = () => CId.From(0);
            config.ConvertToDb = id => id.Cast<int>();
            config.ConvertFromDb = value => CId.From(value);
            config.JsonConverter = value => CId.From(int.Parse(value));
            config.NullableJsonConverter = value => string.IsNullOrWhiteSpace(value) ? null : CId.From(int.Parse(value));
            config.ParseFunction = value => CId.From(int.Parse(value));
            config.ToByteArrayFunction = value => BitConverter.GetBytes(value);
        });
    }
}
