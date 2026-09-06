// Infrastructure/Persistence/NpgsqlConnectionFactory.cs
using System.Data;
using Npgsql;

namespace Infrastructure.Persistence;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

/// <summary>
/// Uses a shared NpgsqlDataSource (enum mappings applied once at startup).
/// </summary>
public sealed class NpgsqlDataSourceConnectionFactory : IDbConnectionFactory
{
    private readonly NpgsqlDataSource _dataSource;

    public NpgsqlDataSourceConnectionFactory(NpgsqlDataSource dataSource)
        => _dataSource = dataSource;

    public IDbConnection CreateConnection()
    {
        var connection = _dataSource.CreateConnection();
        connection.Open();
        return connection;
    }
}