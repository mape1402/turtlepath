namespace TurtlePath.Identifier
{
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Microsoft.EntityFrameworkCore.ValueGeneration;

    /// <summary>
    /// Generates values for <see cref="CId"/> properties when saving entities to the database.
    /// </summary>
    public class CIdDbValueGenerator : ValueGenerator
    {
        /// <summary>
        /// Gets a value indicating whether the values generated are temporary.
        /// </summary>
        public override bool GeneratesTemporaryValues => false;

        /// <summary>
        /// Generates the next value for a <see cref="CId"/> property.
        /// </summary>
        /// <param name="entry">The entity entry for which the value is being generated.</param>
        /// <returns>A new <see cref="CId"/> instance.</returns>
        protected override object NextValue(EntityEntry entry)
            => CIdMetadata.DefaultFactory();
    }
}
