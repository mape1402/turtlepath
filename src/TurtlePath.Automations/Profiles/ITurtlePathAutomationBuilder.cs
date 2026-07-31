namespace TurtlePath.Automations.Profiles
{
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Domain.Identifier;

    /// <summary>
    /// Builds automation descriptors from profile declarations.
    /// </summary>
    public interface ITurtlePathAutomationBuilder
    {
        /// <summary>
        /// Configures automations for a recommended TurtlePath entity using <see cref="CId"/>.
        /// </summary>
        IEntityAutomationBuilder<TEntity, CId> For<TEntity>()
            where TEntity : BaseEntity;

        /// <summary>
        /// Configures automations for a custom entity key contract.
        /// </summary>
        IEntityAutomationBuilder<TEntity, TKey> For<TEntity, TKey>()
            where TEntity : class, IEntity<TKey>;
    }
}
