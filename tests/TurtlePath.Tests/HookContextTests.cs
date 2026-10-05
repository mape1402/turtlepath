using TurtlePath.Hooks;

namespace TurtlePath.Tests;

public sealed class HookContextTests
{
    [Fact]
    public void Command_context_stores_typed_values_and_response()
    {
        var request = new SampleRequest("create");
        var entity = new SampleEntity { Name = "Ada" };
        var response = new SampleResponse("created");
        var key = HookContextKey.Set<string>("stage");
        var numberKey = HookContextKey.Set<int>("attempt");
        var context = new CommandHookContext<SampleRequest, SampleEntity, SampleResponse>(request)
        {
            Entity = entity,
            Response = response
        };

        context.Set(key, "mapped");

        Assert.Same(request, context.Request);
        Assert.Same(entity, context.Entity);
        Assert.Same(response, context.Response);
        Assert.True(context.Has(key));
        Assert.Equal("mapped", context.Get(key));
        Assert.True(context.TryGet(key, out var value));
        Assert.Equal("mapped", value);
        Assert.False(context.TryGet(numberKey, out var number));
        Assert.Equal(0, number);
        Assert.Equal(0, context.Get(numberKey));
    }

    [Fact]
    public void Query_context_stores_typed_values_and_result()
    {
        var query = new SampleRequest("get");
        var result = new SampleResponse("found");
        var key = HookContextKey.Set<string>("stage");
        var context = new QueryHookContext<SampleRequest, SampleResponse>(query)
        {
            Result = result
        };

        context.Set(key, "queried");

        Assert.Same(query, context.Query);
        Assert.Same(result, context.Result);
        Assert.True(context.Has(key));
        Assert.Equal("queried", context.Get(key));
        Assert.True(context.TryGet(key, out var value));
        Assert.Equal("queried", value);
        Assert.False(context.TryGet(HookContextKey.Set<int>("missing"), out var missing));
        Assert.Equal(0, missing);
    }

    [Fact]
    public void Hook_context_key_rejects_default_or_empty_names()
    {
        Assert.Throws<ArgumentException>(() => HookContextKey.Set<string>(" "));
        Assert.Equal("stage", HookContextKey.Set<string>("stage").ToString());

        var command = new CommandHookContext<SampleRequest, SampleEntity>(new SampleRequest("create"));
        var query = new QueryHookContext<SampleRequest, SampleResponse>(new SampleRequest("get"));

        Assert.Throws<ArgumentException>(() => command.Set(default, "value"));
        Assert.Throws<ArgumentException>(() => command.Get(default(HookContextKey<string>)));
        Assert.Throws<ArgumentException>(() => command.Has(default(HookContextKey<string>)));
        Assert.Throws<ArgumentException>(() => command.TryGet(default(HookContextKey<string>), out _));
        Assert.Throws<ArgumentException>(() => query.Set(default, "value"));
        Assert.Throws<ArgumentException>(() => query.Get(default(HookContextKey<string>)));
        Assert.Throws<ArgumentException>(() => query.Has(default(HookContextKey<string>)));
        Assert.Throws<ArgumentException>(() => query.TryGet(default(HookContextKey<string>), out _));
    }

    private sealed record SampleRequest(string Name);

    private sealed record SampleResponse(string Name);

    private sealed class SampleEntity
    {
        public string Name { get; set; }
    }
}
