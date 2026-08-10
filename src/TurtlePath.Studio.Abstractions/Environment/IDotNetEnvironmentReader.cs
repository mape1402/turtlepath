namespace TurtlePath.Studio.Abstractions.Environment;

public interface IDotNetEnvironmentReader
{
    Task<DotNetEnvironmentInfo> ReadAsync(CancellationToken cancellationToken = default);
}
