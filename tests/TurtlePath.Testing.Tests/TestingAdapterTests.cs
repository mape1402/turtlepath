namespace TurtlePath.Testing.Tests
{
    using TurtlePath.Testing.Hooks;
    using TurtlePath.Testing.Mapping;
    using TurtlePath.Testing.Validation;

    public sealed class TestingAdapterTests
    {
        [Fact]
        public async Task Delegate_mapper_supports_sync_async_identity_update_and_error_paths()
        {
            var mapper = new DelegateMapperAdapter()
                .WithMap<SourceModel, DestinationModel>(source => new DestinationModel { Name = source.Name })
                .WithMap<AsyncSourceModel, DestinationModel>((source, _) =>
                    ValueTask.FromResult(new DestinationModel { Name = source.Name }))
                .WithUpdateMap<SourceModel, DestinationModel>((source, destination) => destination.Name = source.Name);
            var destination = new DestinationModel { Name = "before" };

            var mapped = await mapper.MapAsync<SourceModel, DestinationModel>(new SourceModel { Name = "sync" });
            var asyncMapped = await mapper.MapAsync<AsyncSourceModel, DestinationModel>(new AsyncSourceModel { Name = "async" });
            var identity = await mapper.MapAsync<DestinationModel, DestinationModel>(destination);
            await mapper.UpdateMapAsync(new SourceModel { Name = "updated" }, destination);

            Assert.Equal("sync", mapped.Name);
            Assert.Equal("async", asyncMapped.Name);
            Assert.Same(destination, identity);
            Assert.Equal("updated", destination.Name);
            await Assert.ThrowsAsync<ArgumentNullException>(() => mapper.MapAsync<SourceModel, DestinationModel>(null).AsTask());
            await Assert.ThrowsAsync<InvalidOperationException>(() => mapper.MapAsync<UnknownSourceModel, DestinationModel>(new UnknownSourceModel()).AsTask());
            await Assert.ThrowsAsync<ArgumentNullException>(() => mapper.UpdateMapAsync<SourceModel, DestinationModel>(null, destination).AsTask());
            await Assert.ThrowsAsync<ArgumentNullException>(() => mapper.UpdateMapAsync<SourceModel, DestinationModel>(new SourceModel(), null).AsTask());
            await Assert.ThrowsAsync<InvalidOperationException>(() => mapper.UpdateMapAsync(new UnknownSourceModel(), destination).AsTask());
            Assert.Throws<ArgumentNullException>(() => mapper.WithMap<SourceModel, DestinationModel>((Func<SourceModel, DestinationModel>)null));
            Assert.Throws<ArgumentNullException>(() => mapper.WithMap<SourceModel, DestinationModel>((Func<SourceModel, CancellationToken, ValueTask<DestinationModel>>)null));
            Assert.Throws<ArgumentNullException>(() => mapper.WithUpdateMap<SourceModel, DestinationModel>(null));
        }

        [Fact]
        public async Task Delegate_validator_supports_valid_models_custom_validators_and_missing_validator_policy()
        {
            var validated = false;
            var validator = new DelegateValidatorAdapter()
                .WithValidModel<SourceModel>()
                .WithValidator<DestinationModel>((_, _) =>
                {
                    validated = true;
                    return ValueTask.CompletedTask;
                });

            await validator.ValidateAsync(new SourceModel());
            await validator.ValidateAsync(new DestinationModel());
            await validator.ValidateAsync(new UnknownSourceModel());

            validator.AllowMissingValidators = false;

            Assert.True(validated);
            await Assert.ThrowsAsync<InvalidOperationException>(() => validator.ValidateAsync(new UnknownSourceModel()).AsTask());
            Assert.Throws<ArgumentNullException>(() => validator.WithValidator<SourceModel>(null));
        }

        [Fact]
        public void Hook_trace_starts_empty_and_can_be_cleared()
        {
            var trace = new HookTrace();
            var entry = new HookTraceEntry(
                "after-save",
                typeof(SourceModel),
                typeof(DestinationModel),
                typeof(DestinationModel),
                new SourceModel(),
                new DestinationModel(),
                new DestinationModel());

            trace.Clear();

            Assert.Empty(trace.Entries);
            Assert.Equal("after-save", entry.Stage);
            Assert.Equal(typeof(DestinationModel), entry.ResponseType);
        }

        private sealed class SourceModel
        {
            public string Name { get; set; }
        }

        private sealed class AsyncSourceModel
        {
            public string Name { get; set; }
        }

        private sealed class UnknownSourceModel
        {
        }

        private sealed class DestinationModel
        {
            public string Name { get; set; }
        }
    }
}
