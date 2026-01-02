# Testcontainers for .NET Examples

This directory contains practical, runnable examples demonstrating various features and patterns of Testcontainers for .NET.

## Prerequisites

Before running these examples, you need:

1. **.NET 8.0 SDK** or later installed
2. **Docker** running locally
3. Required NuGet packages (restored automatically)

## Examples Overview

### 01_PostgreSqlBasicTests.cs
**Basic PostgreSQL Usage**

Demonstrates:
- Starting a PostgreSQL container with default settings
- Connecting to PostgreSQL with Npgsql
- Custom database configuration (database name, username, password)
- Creating schemas and inserting data
- Using IAsyncLifetime for test lifecycle management

Run with:
```bash
dotnet test --filter "FullyQualifiedName~PostgreSqlBasicTests"
```

### 02_RedisCacheTests.cs
**Redis Operations**

Demonstrates:
- Basic Redis key-value operations with StackExchange.Redis
- Key expiration
- Using RedisBuilder for container configuration
- Proper async cleanup with IAsyncLifetime

Run with:
```bash
dotnet test --filter "FullyQualifiedName~RedisCacheTests"
```

### 03_SqlServerEntityFrameworkTests.cs
**SQL Server with Entity Framework Core**

Demonstrates:
- Using SQL Server container with Entity Framework Core
- Database context initialization
- Entity CRUD operations
- EnsureCreated for schema setup

Run with:
```bash
dotnet test --filter "FullyQualifiedName~SqlServerEntityFrameworkTests"
```

### 04_MultiContainerNetworkTests.cs
**Multi-Container Networking**

Demonstrates:
- Creating custom Docker networks
- Connecting multiple containers on the same network
- Container-to-container communication using network aliases
- Simulating microservices architectures
- Proper cleanup order (containers before networks)

This is essential for:
- Integration testing with multiple services
- Testing service dependencies
- Simulating production-like environments

Run with:
```bash
dotnet test --filter "FullyQualifiedName~MultiContainerNetworkTests"
```

### 05_GenericContainerTests.cs
**Generic Container Patterns**

Demonstrates:
- Using containers without pre-configured modules
- Custom nginx container with HTML content
- Environment variables
- Port mappings
- Different wait strategies (port-based, log-based, HTTP-based)
- Reading container logs
- Executing commands in running containers

Run with:
```bash
dotnet test --filter "FullyQualifiedName~GenericContainerTests"
```

## Running All Examples

To run all examples:

```bash
# Run all tests
dotnet test

# Run all tests with verbose output
dotnet test --logger "console;verbosity=detailed"

# Run a specific test file
dotnet test --filter "FullyQualifiedName~PostgreSqlBasicTests"
```

## Common Patterns

### 1. Basic Pattern (with Module and IAsyncLifetime)

```csharp
using Testcontainers.PostgreSql;
using Xunit;

public class DatabaseTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
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
    public async Task CanConnect()
    {
        var connectionString = _postgres.GetConnectionString();
        // Use connection string...
    }
}
```

### 2. Generic Container Pattern

```csharp
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

var container = new ContainerBuilder()
    .WithImage("image:tag")
    .WithPortBinding(8080, true)
    .WithEnvironment("KEY", "value")
    .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8080))
    .Build();

await container.StartAsync();
```

### 3. Multi-Container Pattern

```csharp
using DotNet.Testcontainers.Networks;

// Create network
var network = new NetworkBuilder().Build();
await network.CreateAsync();

// Start containers on network
var db = new PostgreSqlBuilder()
    .WithNetwork(network)
    .WithNetworkAliases("database")
    .Build();
await db.StartAsync();

var app = new ContainerBuilder()
    .WithImage("myapp:latest")
    .WithNetwork(network)
    .WithEnvironment("DB_HOST", "database")
    .Build();
await app.StartAsync();
```

## Tips and Best Practices

1. **Always use IAsyncLifetime for async setup/teardown**
   ```csharp
   public class Tests : IAsyncLifetime
   {
       public async Task InitializeAsync() { /* setup */ }
       public async Task DisposeAsync() { /* cleanup */ }
   }
   ```

2. **Use pre-configured modules when available**
   - Modules provide sensible defaults
   - Helper methods like `GetConnectionString()`
   - Automatic credential management

3. **Use class fixtures for shared containers**
   - Faster test execution
   - Shared state across multiple tests

4. **Use custom networks for multi-container tests**
   - Containers can communicate via aliases
   - More realistic than host networking

5. **Use appropriate wait strategies**
   - `UntilPortIsAvailable` - when service listens on a port
   - `UntilMessageIsLogged` - when service logs a ready message
   - `UntilHttpRequestIsSucceeded` - when service has an HTTP health endpoint

## Troubleshooting

### Container won't start
- Check if Docker is running: `docker ps`
- Check container logs: `await container.GetLogsAsync()`
- Increase timeout: `.WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(80).WithTimeout(TimeSpan.FromMinutes(2)))`

### Port conflicts
- Use `.WithPortBinding(port, true)` for random host ports
- Avoid fixed port bindings in tests

### Image pull failures
- Pull manually first: `docker pull postgres:16-alpine`
- Check network connectivity
- For private registries: `docker login registry.example.com`

### Cleanup issues
- Verify Ryuk is running: `docker ps | grep ryuk`
- Ensure `WithCleanUp(true)` is set (default)
- Check cleanup order: dispose containers before networks

## Additional Resources

- [Testcontainers for .NET Documentation](https://dotnet.testcontainers.org/)
- [NuGet Packages](https://www.nuget.org/packages?q=testcontainers)
- [GitHub Repository](https://github.com/testcontainers/testcontainers-dotnet)

## Building and Running

This is a standard .NET test project:

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run all tests
dotnet test

# Run specific test
dotnet test --filter "FullyQualifiedName~TestName"
```
