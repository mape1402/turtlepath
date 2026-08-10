namespace DTemplate.Api.Boundaries
{
    /// <summary>
    /// Marks a Spider request type that should not run inside the transaction boundary.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class SkipTransactionBoundaryAttribute : Attribute
    {
    }
}
