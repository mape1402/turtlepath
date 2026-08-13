namespace TurtlePath.Spider;

/// <summary>
/// Defines a typed configuration profile for a Spider boundary integration.
/// </summary>
/// <typeparam name="TOptions">The options type owned by the boundary integration.</typeparam>
public interface ISpiderBoundaryProfile<in TOptions>
    where TOptions : class
{
    /// <summary>
    /// Configures the boundary options.
    /// </summary>
    /// <param name="options">The options instance.</param>
    void Configure(TOptions options);
}
