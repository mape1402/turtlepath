using TurtlePath.Domain.Identifier;

namespace Heroes.Service.Domain.Contracts;

/// <summary>
/// Shared domain contract used by services that operate on both heroes and villains.
/// </summary>
public interface ITeamMember
{
    /// <summary>
    /// Gets or sets the team identifier exposed as <c>CId</c> regardless of the configured storage type.
    /// </summary>
    CId TeamId { get; set; }
}
