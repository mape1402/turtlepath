namespace TurtlePath.Automations.Profiles
{
    using TurtlePath.Automations.Descriptors;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Domain.Identifier;

    internal sealed class AutomationProfileDescriptorBuilder : ITurtlePathAutomationBuilder
    {
        private readonly AutomationDescriptorRegistry registry = new();

        public IReadOnlyCollection<AutomationDescriptor> Descriptors => registry.Descriptors;

        public IEntityAutomationBuilder<TEntity, CId> For<TEntity>()
            where TEntity : BaseEntity
            => new EntityAutomationBuilder<TEntity, CId>(registry);

        public IEntityAutomationBuilder<TEntity, TKey> For<TEntity, TKey>()
            where TEntity : class, IEntity<TKey>
            => new EntityAutomationBuilder<TEntity, TKey>(registry);

        public static IReadOnlyCollection<AutomationDescriptor> Build(TurtlePathAutomationProfile profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            var builder = new AutomationProfileDescriptorBuilder();
            profile.Configure(builder);

            return builder.Descriptors;
        }
    }
}
