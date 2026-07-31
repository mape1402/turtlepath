namespace TurtlePath.Commands.Steps
{
    /// <summary>
    /// Default patch step that delegates patch behavior to the request.
    /// </summary>
    internal sealed class DefaultEntityPatchStep<TRequest, TEntity> : IEntityPatchStep<TRequest, TEntity>
        where TEntity : class
    {
        public ValueTask PatchAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
        {
            if (request is IPatchAction<TEntity> patchAction)
                return patchAction.PatchAsync(entity, cancellationToken);

            throw new InvalidOperationException(
                $"Request type '{typeof(TRequest).FullName}' must implement '{typeof(IPatchAction<TEntity>).FullName}' to use the default TurtlePath patch flow.");
        }
    }
}
