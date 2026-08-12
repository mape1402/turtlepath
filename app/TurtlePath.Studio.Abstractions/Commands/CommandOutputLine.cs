namespace TurtlePath.Studio.Abstractions.Commands;

public sealed record CommandOutputLine(
    CommandOutputKind Kind,
    string Text,
    DateTimeOffset Timestamp);
