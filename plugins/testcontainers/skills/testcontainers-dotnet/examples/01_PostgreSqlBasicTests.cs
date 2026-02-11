using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace TestcontainersExamples;

/// <summary>
/// Demonstrates basic PostgreSQL container usage with Testcontainers for .NET
/// </summary>
public class PostgreSqlBasicTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task ConnectionTest_ShouldConnect()
    {
        // Arrange
        var connectionString = _postgres.GetConnectionString();

        // Act
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        // Assert
        Assert.NotNull(connection);
        Assert.Equal(System.Data.ConnectionState.Open, connection.State);
    }

    [Fact]
    public async Task SimpleQuery_ShouldReturnResult()
    {
        // Arrange
        var connectionString = _postgres.GetConnectionString();
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        // Act
        await using var command = new NpgsqlCommand("SELECT 1 + 1", connection);
        var result = await command.ExecuteScalarAsync();

        // Assert
        Assert.Equal(2, result);
    }
}

/// <summary>
/// Demonstrates PostgreSQL with custom configuration
/// </summary>
public class PostgreSqlCustomConfigTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("testdb")
        .WithUsername("testuser")
        .WithPassword("testpass")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    [Fact]
    public void ConnectionString_ShouldContainCustomValues()
    {
        // Act
        var connectionString = _postgres.GetConnectionString();

        // Assert
        Assert.Contains("testuser", connectionString);
        Assert.Contains("testpass", connectionString);
        Assert.Contains("testdb", connectionString);
    }

    [Fact]
    public async Task CreateTableAndInsert_ShouldPersist()
    {
        // Arrange
        var connectionString = _postgres.GetConnectionString();
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        // Create table
        await using (var createCommand = new NpgsqlCommand(@"
            CREATE TABLE users (
                id SERIAL PRIMARY KEY,
                name TEXT NOT NULL,
                email TEXT UNIQUE NOT NULL,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            )", connection))
        {
            await createCommand.ExecuteNonQueryAsync();
        }

        // Insert data
        await using (var insertCommand = new NpgsqlCommand(
            "INSERT INTO users (name, email) VALUES (@name, @email) RETURNING id",
            connection))
        {
            insertCommand.Parameters.AddWithValue("name", "Alice");
            insertCommand.Parameters.AddWithValue("email", "alice@example.com");

            var userId = await insertCommand.ExecuteScalarAsync();

            // Assert insert succeeded
            Assert.NotNull(userId);
        }

        // Query data
        await using (var selectCommand = new NpgsqlCommand(
            "SELECT name, email FROM users WHERE email = @email",
            connection))
        {
            selectCommand.Parameters.AddWithValue("email", "alice@example.com");

            await using var reader = await selectCommand.ExecuteReaderAsync();
            Assert.True(await reader.ReadAsync());

            var name = reader.GetString(0);
            var email = reader.GetString(1);

            Assert.Equal("Alice", name);
            Assert.Equal("alice@example.com", email);
        }
    }
}
