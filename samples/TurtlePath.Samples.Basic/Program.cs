using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using TurtlePath.Domain.Identifier;
using TurtlePath.EntityFrameworkCore;
using TurtlePath.Hooks;

var services = new ServiceCollection();

services
    .AddTurtlePath(typeof(CreateTodoAuditHook).Assembly)
    .UseCId<Guid, string>(config =>
    {
        config.DefaultFactory = () => new CId(Guid.NewGuid());
        config.ConvertToDb = id => id.ToString();
        config.ConvertFromDb = value => CId.Parse(value);
        config.JsonConverter = value => CId.Parse(value);
        config.NullableJsonConverter = value => string.IsNullOrWhiteSpace(value) ? null : CId.Parse(value);
        config.ParseFunction = value => new CId(Guid.Parse(value));
        config.ToByteArrayFunction = value => value.ToByteArray();
    })
    .UseEntityFrameworkCore(options => options with
    {
        ConfigurationAssemblies = [typeof(CreateTodoAuditHook).Assembly]
    });

using var provider = services.BuildServiceProvider();
var idFactory = provider.GetRequiredService<ICIdFactory>();
var hooks = provider.GetServices<IBeforeValidationHook<CreateTodoRequest, Todo>>().ToArray();
var dbContextOptions = provider.GetRequiredService<TurtlePathDbContextOptions>();

Console.WriteLine($"Generated TurtlePath CId: {idFactory.New()}");
Console.WriteLine($"Registered TurtlePath hooks: {hooks.Length}");
Console.WriteLine($"Registered TurtlePath EF configuration assemblies: {dbContextOptions.ConfigurationAssemblies.Count}");

public sealed class CreateTodoRequest
{
    public string Title { get; set; } = string.Empty;
}

public sealed class Todo
{
    public CId Id { get; set; } = CId.New();
    public string Title { get; set; } = string.Empty;
}

public sealed class TodoDbContext : BaseDbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options, TurtlePathDbContextOptions turtlePathOptions)
        : base(options, turtlePathOptions)
    {
    }

    public DbSet<Todo> Todos => Set<Todo>();
}

public sealed class CreateTodoAuditHook : IBeforeValidationHook<CreateTodoRequest, Todo>
{
    public ValueTask BeforeValidationAsync(
        CommandHookContext<CreateTodoRequest, Todo> context,
        CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;
}

