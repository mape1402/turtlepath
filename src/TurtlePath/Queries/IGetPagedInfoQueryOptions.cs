namespace TurtlePath.Queries
{
    /// <summary>
    /// Provides query-specific options for generic paged handlers.
    /// </summary>
    public interface IGetPagedInfoQueryOptions<TQuery, TEntity>
    {
        /// <summary>
        /// Gets the default sort expression when the request does not provide one.
        /// </summary>
        string DefaultSorts { get; }
    }
}
