namespace TurtlePath.Automations.Options
{
    using TurtlePath.Automations.Descriptors;
    using TurtlePath.Queries;

    internal sealed class DescriptorGetManyQueryOptions<TQuery, TEntity, TResponse> : IGetManyQueryOptions<TQuery, TEntity>
    {
        public DescriptorGetManyQueryOptions(AutomationDescriptorRegistry registry)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            DefaultSorts = registry
                .Find(typeof(TQuery), typeof(IEnumerable<TResponse>))
                ?.DefaultSortProperty;
        }

        public string DefaultSorts { get; }
    }
}
