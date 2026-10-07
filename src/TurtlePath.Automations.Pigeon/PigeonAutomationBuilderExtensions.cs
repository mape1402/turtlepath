namespace TurtlePath.Automations.Pigeon;

using TurtlePath.Automations.Profiles;
using TurtlePath.Domain.Contracts;

/// <summary>
/// Provides Pigeon metadata extensions for TurtlePath automation profiles.
/// </summary>
public static class PigeonAutomationBuilderExtensions
{
    /// <summary>
    /// Exposes a mutation automation request as a Pigeon consumer.
    /// </summary>
    /// <typeparam name="TRequest">The automation request type.</typeparam>
    /// <typeparam name="TEntity">The entity type handled by the automation.</typeparam>
    /// <typeparam name="TKey">The entity key type.</typeparam>
    /// <param name="builder">The mutation automation builder.</param>
    /// <param name="topic">The Pigeon topic consumed by the request.</param>
    /// <param name="version">The semantic message contract version.</param>
    /// <param name="subscription">The optional subscription, queue name, or consumer group.</param>
    /// <returns>The same automation builder.</returns>
    public static IMutationAutomationBuilder<TRequest, TEntity, TKey> Consume<TRequest, TEntity, TKey>(
        this IMutationAutomationBuilder<TRequest, TEntity, TKey> builder,
        string topic,
        string version = "1.0.0",
        string subscription = null)
        where TEntity : class, IEntity<TKey>
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.SetMetadata(
            AutomationPigeonMetadata.Consumer,
            new AutomationPigeonConsumerOptions(topic, version, subscription));

        return builder;
    }

    /// <summary>
    /// Exposes a query automation request as a Pigeon consumer.
    /// </summary>
    /// <typeparam name="TQuery">The automation query type.</typeparam>
    /// <typeparam name="TEntity">The entity type handled by the automation.</typeparam>
    /// <typeparam name="TKey">The entity key type.</typeparam>
    /// <param name="builder">The query automation builder.</param>
    /// <param name="topic">The Pigeon topic consumed by the query.</param>
    /// <param name="version">The semantic message contract version.</param>
    /// <param name="subscription">The optional subscription, queue name, or consumer group.</param>
    /// <returns>The same automation builder.</returns>
    public static IQueryAutomationBuilder<TQuery, TEntity, TKey> Consume<TQuery, TEntity, TKey>(
        this IQueryAutomationBuilder<TQuery, TEntity, TKey> builder,
        string topic,
        string version = "1.0.0",
        string subscription = null)
        where TEntity : class, IEntity<TKey>
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.SetMetadata(
            AutomationPigeonMetadata.Consumer,
            new AutomationPigeonConsumerOptions(topic, version, subscription));

        return builder;
    }
}
