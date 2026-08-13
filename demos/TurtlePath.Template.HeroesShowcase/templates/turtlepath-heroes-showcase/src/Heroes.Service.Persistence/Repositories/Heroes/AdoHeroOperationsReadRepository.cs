using System.Globalization;
using Microsoft.Data.Sqlite;
using TurtlePath.Domain.Identifier;

namespace Heroes.Service.Persistence.Repositories.Heroes;

/// <summary>
/// ADO.NET repository for reporting queries that do not need the normal EF/TurtlePath query path.
/// </summary>
public sealed class AdoHeroOperationsReadRepository : IHeroOperationsReadRepository
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdoHeroOperationsReadRepository"/> class.
    /// </summary>
    /// <param name="connectionString">The SQLite connection string used by the showcase database.</param>
    public AdoHeroOperationsReadRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<HeroOperationsReadRow>> GetActiveHeroOperationsAsync(CId? teamId, CancellationToken cancellationToken = default)
    {
        var rows = new List<HeroOperationsReadRow>();
        await using var connection = new SqliteConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT h.Alias,
                   h.City,
                   h.PowerLevel,
                   t.Name AS TeamName,
                   COUNT(i.Id) AS AssignedIncidents
              FROM Heroes h
              LEFT JOIN Teams t ON t.Id = h.TeamId
              LEFT JOIN Incidents i ON i.AssignedHeroId = h.Id AND i.Status <> 'Resolved'
             WHERE h.Active = 1
               AND (@TeamId IS NULL OR h.TeamId = @TeamId)
             GROUP BY h.Id, h.Alias, h.City, h.PowerLevel, t.Name
             ORDER BY h.PowerLevel DESC, h.Alias ASC
            """;

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@TeamId";
        parameter.Value = teamId is null ? DBNull.Value : teamId.Value.ToString();
        command.Parameters.Add(parameter);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new HeroOperationsReadRow
            {
                Alias = reader.GetString(0),
                City = reader.GetString(1),
                PowerLevel = reader.GetInt32(2),
                TeamName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                AssignedOpenIncidents = Convert.ToInt32(reader.GetValue(4), CultureInfo.InvariantCulture)
            });
        }

        return rows;
    }
}
