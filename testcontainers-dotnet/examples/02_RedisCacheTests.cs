using StackExchange.Redis;
using Testcontainers.Redis;
using Xunit;

namespace TestcontainersExamples;

/// <summary>
/// Demonstrates Redis container usage for caching scenarios
/// </summary>
public class RedisCacheTests : IAsyncLifetime
{
    private readonly RedisContainer _redis = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    private IConnectionMultiplexer? _connection;
    private IDatabase? _db;

    public async Task InitializeAsync()
    {
        await _redis.StartAsync();
        _connection = await ConnectionMultiplexer.ConnectAsync(_redis.GetConnectionString());
        _db = _connection.GetDatabase();
    }

    public async Task DisposeAsync()
    {
        _connection?.Dispose();
        await _redis.DisposeAsync();
    }

    [Fact]
    public async Task SetAndGet_ShouldStoreAndRetrieveValue()
    {
        // Arrange
        const string key = "test:key1";
        const string value = "test-value-1";

        // Act
        await _db!.StringSetAsync(key, value);
        var retrievedValue = await _db.StringGetAsync(key);

        // Assert
        Assert.Equal(value, retrievedValue);
    }

    [Fact]
    public async Task SetWithExpiration_ShouldExpireKey()
    {
        // Arrange
        const string key = "test:expiring-key";
        const string value = "temporary-value";

        // Act
        await _db!.StringSetAsync(key, value, TimeSpan.FromSeconds(1));
        var valueBefore = await _db.StringGetAsync(key);

        // Wait for expiration
        await Task.Delay(TimeSpan.FromSeconds(2));

        var valueAfter = await _db.StringGetAsync(key);

        // Assert
        Assert.Equal(value, valueBefore.ToString());
        Assert.True(valueAfter.IsNull);
    }

    [Fact]
    public async Task Increment_ShouldIncrementCounter()
    {
        // Arrange
        const string key = "test:counter";

        // Act
        var count1 = await _db!.StringIncrementAsync(key);
        var count2 = await _db.StringIncrementAsync(key);
        var count3 = await _db.StringIncrementAsync(key);

        // Assert
        Assert.Equal(1, count1);
        Assert.Equal(2, count2);
        Assert.Equal(3, count3);
    }

    [Fact]
    public async Task HashOperations_ShouldStoreAndRetrieveFields()
    {
        // Arrange
        const string key = "test:user:1";

        // Act
        await _db!.HashSetAsync(key, new HashEntry[]
        {
            new("name", "Alice"),
            new("email", "alice@example.com"),
            new("age", "30")
        });

        var name = await _db.HashGetAsync(key, "name");
        var email = await _db.HashGetAsync(key, "email");
        var age = await _db.HashGetAsync(key, "age");

        // Assert
        Assert.Equal("Alice", name.ToString());
        Assert.Equal("alice@example.com", email.ToString());
        Assert.Equal("30", age.ToString());
    }
}
