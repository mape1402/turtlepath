using Microsoft.Extensions.DependencyInjection;
using TurtlePath.Domain.Identifier;
using TurtlePath.Hooks;

var services = new ServiceCollection();

services.AddTurtlePath<Guid, string>(
    config =>
    {
        config.DefaultFactory = () => new CId(Guid.NewGuid());
        config.ConvertToDb = id => id.ToString();
        config.ConvertFromDb = value => CId.Parse(value);
        config.JsonConverter = value => CId.Parse(value);
        config.NullableJsonConverter = value => string.IsNullOrWhiteSpace(value) ? null : CId.Parse(value);
        config.ParseFunction = value => new CId(Guid.Parse(value));
        config.ToByteArrayFunction = value => value.ToByteArray();
    },
    typeof(CreateTodoAuditHook).Assembly);

using var provider = services.BuildServiceProvider();
var idFactory = provider.GetRequiredService<ICIdFactory>();
var hooks = provider.GetServices<IBeforeValidationHook<CreateTodoRequest, Todo>>().ToArray();

Console.WriteLine($"Generated TurtlePath CId: {idFactory.New()}");
Console.WriteLine($"Registered TurtlePath hooks: {hooks.Length}");

public sealed class CreateTodoRequest
{
    public string Title { get; set; } = string.Empty;
}

public sealed class Todo
{
    public CId Id { get; set; } = CId.New();
    public string Title { get; set; } = string.Empty;
}

public sealed class CreateTodoAuditHook : IBeforeValidationHook<CreateTodoRequest, Todo>
{
    public ValueTask BeforeValidationAsync(
        CommandHookContext<CreateTodoRequest, Todo> context,
        CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;
}

