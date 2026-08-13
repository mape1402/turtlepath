namespace Heroes.Service.Domain.Enums;

/// <summary>
/// Defines which side owns a skill.
/// </summary>
public enum Alignment
{
    /// <summary>
    /// Skill owned by a hero.
    /// </summary>
    Hero = 1,

    /// <summary>
    /// Skill owned by a villain.
    /// </summary>
    Villain = 2
}
