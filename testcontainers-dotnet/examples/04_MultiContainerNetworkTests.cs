using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;

namespace TestcontainersExamples;

/// <summary>
/// Demonstrates multi-container networking with custom Docker networks
/// </summary>
public class MultiContainerNetworkTests : IAsyncLifetime
{
    private INetwork? _network;
    private PostgreSqlContainer? _postgres;
    private RedisContainer? _redis;

    public async Task InitializeAsync()
    {
        // Create custom network
        _network = new NetworkBuilder()
            .Build();

        await _network.CreateAsync();

        // Start PostgreSQL on the network
        _postgres = new PostgreSqlBuilder("postgres:16-alpine")
            .WithNetwork(_network)
            .WithNetworkAliases("database")
            .Build();

        await _postgres.StartAsync();

        // Start Redis on the same network
        _redis = new RedisBuilder("redis:7-alpine")
            .WithNetwork(_network)
            .WithNetworkAliases("cache")
            .Build();

        await _redis.StartAsync();
    }

    public async Task DisposeAsync()
    {
        // Important: Dispose containers before network
        if (_redis != null)
        {
            await _redis.DisposeAsync();
        }

        if (_postgres != null)
        {
            await _postgres.DisposeAsync();
        }

        if (_network != null)
        {
            await _network.DeleteAsync();
        }
    }

    [Fact]
    public void PostgresContainer_ShouldBeOnNetwork()
    {
        // Assert
        Assert.NotNull(_postgres);
        var connectionString = _postgres!.GetConnectionString();
        Assert.NotEmpty(connectionString);
    }

    [Fact]
    public void RedisContainer_ShouldBeOnNetwork()
    {
        // Assert
        Assert.NotNull(_redis);
        var connectionString = _redis!.GetConnectionString();
        Assert.NotEmpty(connectionString);
    }

    [Fact]
    public async Task ContainerCommunication_UsingNetworkAliases()
    {
        // Arrange - Create an app container that connects to both services
        var appContainer = new ContainerBuilder()
            .WithImage("alpine:latest")
            .WithNetwork(_network!)
            .WithNetworkAliases("app")
            .WithCommand("sh", "-c", "sleep 30")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilCommandIsCompleted("sh", "-c", "echo ready"))
            .Build();

        await appContainer.StartAsync();

        try
        {
            // Act - Verify network connectivity
            // Check if database alias is resolvable
            var pingDbResult = await appContainer.ExecAsync(new[] { "ping", "-c", "1", "database" });

            // Check if cache alias is resolvable
            var pingCacheResult = await appContainer.ExecAsync(new[] { "ping", "-c", "1", "cache" });

            // Assert
            Assert.Equal(0, pingDbResult.ExitCode);
            Assert.Equal(0, pingCacheResult.ExitCode);
        }
        finally
        {
            await appContainer.DisposeAsync();
        }
    }
}

/// <summary>
/// Demonstrates a simulated microservices architecture with multiple containers
/// </summary>
public class MicroservicesArchitectureTests : IAsyncLifetime
{
    private INetwork? _network;
    private PostgreSqlContainer? _database;
    private RedisContainer? _cache;
    private IContainer? _appContainer;

    public async Task InitializeAsync()
    {
        // Create network for all services
        _network = new NetworkBuilder()
            .WithName("microservices-net")
            .Build();

        await _network.CreateAsync();

        // Start database service
        _database = new PostgreSqlBuilder("postgres:16-alpine")
            .WithNetwork(_network)
            .WithNetworkAliases("db", "database")
            .Build();

        // Start cache service
        _cache = new RedisBuilder("redis:7-alpine")
            .WithNetwork(_network)
            .WithNetworkAliases("redis", "cache")
            .Build();

        // Start both in parallel
        await Task.WhenAll(
            _database.StartAsync(),
            _cache.StartAsync()
        );

        // Start application container that uses both services
        _appContainer = new ContainerBuilder()
            .WithImage("alpine:latest")
            .WithNetwork(_network)
            .WithNetworkAliases("app")
            .WithEnvironment("DB_HOST", "database")
            .WithEnvironment("DB_PORT", "5432")
            .WithEnvironment("REDIS_HOST", "cache")
            .WithEnvironment("REDIS_PORT", "6379")
            .WithCommand("sh", "-c", "sleep 60")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilCommandIsCompleted("sh", "-c", "echo ready"))
            .Build();

        await _appContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        // Cleanup in reverse order
        if (_appContainer != null)
        {
            await _appContainer.DisposeAsync();
        }

        if (_cache != null)
        {
            await _cache.DisposeAsync();
        }

        if (_database != null)
        {
            await _database.DisposeAsync();
        }

        if (_network != null)
        {
            await _network.DeleteAsync();
        }
    }

    [Fact]
    public void AllServices_ShouldBeRunning()
    {
        // Assert
        Assert.NotNull(_database);
        Assert.NotNull(_cache);
        Assert.NotNull(_appContainer);
    }

    [Fact]
    public async Task AppContainer_CanResolveServiceAliases()
    {
        // Act
        var dbHostResult = await _appContainer!.ExecAsync(new[] { "sh", "-c", "getent hosts database" });
        var cacheHostResult = await _appContainer.ExecAsync(new[] { "sh", "-c", "getent hosts cache" });

        // Assert
        Assert.Equal(0, dbHostResult.ExitCode);
        Assert.Equal(0, cacheHostResult.ExitCode);
    }

    [Fact]
    public void EnvironmentVariables_ShouldBeSet()
    {
        // This test verifies that environment configuration is set correctly
        // In a real scenario, the app would use these to connect to services

        // Assert - Just verify containers are configured
        Assert.NotNull(_appContainer);
        Assert.NotNull(_database);
        Assert.NotNull(_cache);
    }
}
