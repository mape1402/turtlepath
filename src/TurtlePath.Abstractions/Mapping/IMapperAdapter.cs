namespace TurtlePath.Mapping
{
    /// <summary>
    /// Defines an abstraction for mapping objects between types.
    /// </summary>
    public interface IMapperAdapter
    {
        /// <summary>
        /// Asynchronously maps the source object to a new destination object of the specified type.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TDestination">The type of the destination object.</typeparam>
        /// <param name="source">The source object to map.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A ValueTask representing the asynchronous operation, with the mapped destination object as the result.</returns>
        ValueTask<TDestination> MapAsync<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default)
            where TSource : class
            where TDestination : class;

        /// <summary>
        /// Asynchronously maps the source object onto an existing destination object, updating its values.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TDestination">The type of the destination object.</typeparam>
        /// <param name="source">The source object to map from.</param>
        /// <param name="destination">The destination object to update.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A ValueTask representing the asynchronous operation.</returns>
        ValueTask UpdateMapAsync<TSource, TDestination>(TSource source, TDestination destination, CancellationToken cancellationToken = default)
            where TSource : class
            where TDestination : class;
    }
}
