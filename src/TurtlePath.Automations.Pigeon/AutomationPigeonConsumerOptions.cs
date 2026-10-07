namespace TurtlePath.Automations.Pigeon;

/// <summary>
/// Configures a Pigeon consumer generated from an automation descriptor.
/// </summary>
public sealed record AutomationPigeonConsumerOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AutomationPigeonConsumerOptions"/> class.
    /// </summary>
    /// <param name="topic">The Pigeon topic consumed by the automation request.</param>
    /// <param name="version">The semantic message contract version.</param>
    /// <param name="subscription">The optional subscription, queue name, or consumer group.</param>
    public AutomationPigeonConsumerOptions(string topic, string version = "1.0.0", string subscription = null)
    {
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("Topic cannot be null or empty.", nameof(topic));

        if (string.IsNullOrWhiteSpace(version))
            throw new ArgumentException("Version cannot be null or empty.", nameof(version));

        Topic = topic;
        Version = version;
        Subscription = subscription;
    }

    /// <summary>
    /// Gets the Pigeon topic consumed by the automation request.
    /// </summary>
    public string Topic { get; }

    /// <summary>
    /// Gets the semantic message contract version.
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// Gets the optional subscription, queue name, or consumer group.
    /// </summary>
    public string Subscription { get; }
}
