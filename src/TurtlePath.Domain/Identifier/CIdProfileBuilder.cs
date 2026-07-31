namespace TurtlePath.Domain.Identifier
{
    /// <summary>
    /// Builds CId definitions for a profile.
    /// </summary>
    public sealed class CIdProfileBuilder
    {
        private readonly CIdDefinitionRegistry registry;

        /// <summary>
        /// Initializes a new instance of the <see cref="CIdProfileBuilder"/> class.
        /// </summary>
        /// <param name="registry">The definition registry that receives configured definitions.</param>
        public CIdProfileBuilder(CIdDefinitionRegistry registry)
        {
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        /// <summary>
        /// Configures the default identifier definition.
        /// </summary>
        /// <typeparam name="TTargetType">The domain identifier value type.</typeparam>
        /// <typeparam name="TDbType">The database identifier value type.</typeparam>
        /// <param name="configureIdentifier">The CId configuration callback.</param>
        /// <returns>The same CId profile builder.</returns>
        public CIdProfileBuilder UseCId<TTargetType, TDbType>(
            Action<CIdConfiguration<TTargetType, TDbType>> configureIdentifier)
        {
            AddCIdDefinition(
                CIdDefinition.DefaultContext,
                null,
                CIdDefinition.DefaultPropertyName,
                configureIdentifier);

            return this;
        }

        /// <summary>
        /// Configures an identifier definition for a specific entity.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <typeparam name="TTargetType">The domain identifier value type.</typeparam>
        /// <typeparam name="TDbType">The database identifier value type.</typeparam>
        /// <param name="configureIdentifier">The CId configuration callback.</param>
        /// <param name="propertyName">The identifier property name.</param>
        /// <returns>The same CId profile builder.</returns>
        public CIdProfileBuilder UseCIdFor<TEntity, TTargetType, TDbType>(
            Action<CIdConfiguration<TTargetType, TDbType>> configureIdentifier,
            string propertyName = CIdDefinition.DefaultPropertyName)
        {
            AddCIdDefinition(
                CreateEntityContext(typeof(TEntity), propertyName),
                typeof(TEntity),
                propertyName,
                configureIdentifier);

            return this;
        }

        private void AddCIdDefinition<TTargetType, TDbType>(
            string context,
            Type entityType,
            string propertyName,
            Action<CIdConfiguration<TTargetType, TDbType>> setup)
        {
            if (setup == null)
                throw new ArgumentNullException(nameof(setup));

            var config = new CIdConfiguration<TTargetType, TDbType>();
            setup(config);
            config.ValidateAndThrow();

            registry.Register(new CIdDefinition(
                context,
                entityType,
                propertyName,
                typeof(TTargetType),
                config.DefaultFactory,
                config.ParseFunction,
                id => id.ToString(),
                id => config.ToByteArrayFunction((TTargetType)id.Value),
                config.GenerationStrategy,
                typeof(TDbType),
                config.DbType,
                config.ConvertToDb,
                config.ConvertFromDb));
        }

        private static string CreateEntityContext(Type entityType, string propertyName)
            => $"{entityType.FullName}.{propertyName ?? CIdDefinition.DefaultPropertyName}";
    }
}
