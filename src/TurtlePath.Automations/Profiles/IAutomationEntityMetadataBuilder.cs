namespace TurtlePath.Automations.Profiles
{
    /// <summary>
    /// Allows extension packages to attach integration metadata to every automation operation declared for one entity.
    /// </summary>
    public interface IAutomationEntityMetadataBuilder
    {
        /// <summary>
        /// Adds or replaces a metadata value used by an integration package.
        /// </summary>
        /// <param name="key">The metadata key owned by the integration package.</param>
        /// <param name="value">The metadata value.</param>
        void SetMetadata(string key, object value);
    }
}
