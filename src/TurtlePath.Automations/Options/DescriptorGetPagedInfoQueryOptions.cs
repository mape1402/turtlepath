namespace TurtlePath.Automations.Options
{
    using TurtlePath.Automations.Descriptors;
    using TurtlePath.Models.Responses;
    using TurtlePath.Queries;

    internal sealed class DescriptorGetPagedInfoQueryOptions<TQuery, TEntity, TResponse> : IGetPagedInfoQueryOptions<TQuery, TEntity>
    {
        public DescriptorGetPagedInfoQueryOptions(AutomationDescriptorRegistry registry)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            DefaultSorts = registry
                .Find(typeof(TQuery), typeof(PagedResponse<TResponse>))
                ?.DefaultSortProperty;
        }

        public string DefaultSorts { get; }
    }
}
