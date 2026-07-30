using Microsoft.Extensions.DependencyInjection;
using TurtlePath.Domain.Identifier;

var services = new ServiceCollection();

services.UseCId<Guid, string>(config =>
{
    config.DefaultFactory = () => new CId(Guid.NewGuid());
    config.ConvertToDb = id => id.ToString();
    config.ConvertFromDb = value => CId.Parse(value);
    config.JsonConverter = value => CId.Parse(value);
    config.NullableJsonConverter = value => string.IsNullOrWhiteSpace(value) ? null : CId.Parse(value);
    config.ParseFunction = value => new CId(Guid.Parse(value));
    config.ToByteArrayFunction = value => value.ToByteArray();
});

Console.WriteLine($"Generated TurtlePath CId: {CId.New()}");

