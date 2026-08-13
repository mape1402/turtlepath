namespace TurtlePath.Spider;

/// <summary>
/// Provides a base class for typed Spider boundary configuration profiles.
/// </summary>
/// <typeparam name="TOptions">The options type owned by the boundary integration.</typeparam>
public abstract class SpiderBoundaryProfile<TOptions> : ISpiderBoundaryProfile<TOptions>
    where TOptions : class
{
    /// <inheritdoc />
    public abstract void Configure(TOptions options);
}
