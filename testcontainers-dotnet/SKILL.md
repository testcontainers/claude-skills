---
name: testcontainers-dotnet
description: A comprehensive guide for using Testcontainers for .NET to write reliable integration tests with Docker containers in .NET projects. Supports 40+ pre-configured modules for databases, message queues, cloud services, and more.
license: MIT
---

# Testcontainers for .NET Integration Testing

A comprehensive guide for using Testcontainers for .NET to write reliable integration tests with Docker containers in .NET projects.

## Description

This skill helps you write integration tests using Testcontainers for .NET, a .NET library that provides lightweight, throwaway instances of common databases, message queues, web browsers, or anything that can run in a Docker container.

**Key capabilities:**
- Use 40+ pre-configured modules for common services (databases, message queues, cloud services, etc.)
- Set up and manage Docker containers in .NET tests (xUnit, NUnit, MSTest)
- Configure networking, volumes, and environment variables
- Implement proper cleanup and resource management
- Debug and troubleshoot container issues

## When to Use This Skill

Use this skill when you need to:
- Write integration tests that require real services (databases, message queues, etc.)
- Test against multiple versions or configurations of dependencies
- Create reproducible test environments
- Avoid mocking external dependencies in integration tests
- Set up ephemeral test infrastructure

## Prerequisites

- **Docker or Podman** installed and running
- **.NET 8.0+** (check project requirements; library supports .NET 6.0, 7.0, 8.0, and 9.0)
- **Docker socket** accessible at standard locations (Docker Desktop on macOS/Windows, `/var/run/docker.sock` on Linux)
- **Test framework**: xUnit, NUnit, or MSTest

## Instructions

### 1. Installation & Setup

Add Testcontainers for .NET to your test project:

```bash
# Core library (required)
dotnet add package Testcontainers

# For pre-configured modules (recommended)
# PostgreSQL
dotnet add package Testcontainers.PostgreSql

# SQL Server
dotnet add package Testcontainers.MsSql

# MySQL
dotnet add package Testcontainers.MySql

# MongoDB
dotnet add package Testcontainers.MongoDB

# Redis
dotnet add package Testcontainers.Redis

# Kafka
dotnet add package Testcontainers.Kafka

# RabbitMQ
dotnet add package Testcontainers.RabbitMq

# Elasticsearch
dotnet add package Testcontainers.Elasticsearch

# And many more...
```

**Verify Docker availability:**

```csharp
using DotNet.Testcontainers.Configurations;

[Fact]
public async Task DockerIsAvailable()
{
    // This will throw if Docker is not running
    var testcontainersConfiguration = TestcontainersSettings.OS;
    Assert.NotNull(testcontainersConfiguration);
}
```

---

### 2. Using Pre-Configured Modules (Recommended Approach)

**Testcontainers for .NET provides 40+ pre-configured modules** that offer production-ready configurations, sensible defaults, and helper methods. **Always prefer modules over generic containers** when available.

#### Why Use Modules?

- **Sensible defaults**: Pre-configured ports, environment variables, and wait strategies
- **Connection helpers**: Built-in properties like `GetConnectionString()`, `GetBootstrapAddress()`
- **Specialized features**: Module-specific functionality (e.g., SQL Server with Azure SQL Edge support)
- **Automatic credentials**: Secure credential generation and management
- **Battle-tested**: Used in production by thousands of projects

#### Available Module Categories

**Databases (15+ modules):**
- `Testcontainers.PostgreSql`, `Testcontainers.MsSql`, `Testcontainers.MySql`
- `Testcontainers.MariaDb`, `Testcontainers.MongoDB`, `Testcontainers.Redis`
- `Testcontainers.Oracle`, `Testcontainers.Db2`, `Testcontainers.Cassandra`
- `Testcontainers.CouchDb`, `Testcontainers.ClickHouse`, `Testcontainers.DynamoDb`
- `Testcontainers.InfluxDb`, `Testcontainers.CosmosDb`, `Testcontainers.FaunaDb`

**Message Queues (5+ modules):**
- `Testcontainers.Kafka`, `Testcontainers.RabbitMq`, `Testcontainers.Redpanda`
- `Testcontainers.Pulsar`, `Testcontainers.NATS`

**Search & Storage (5+ modules):**
- `Testcontainers.Elasticsearch`, `Testcontainers.Minio`
- `Testcontainers.Azurite`, `Testcontainers.LocalStack`
- `Testcontainers.Qdrant`

**Cloud & Infrastructure (5+ modules):**
- `Testcontainers.LocalStack` (AWS services)
- `Testcontainers.Azurite` (Azure Storage)
- `Testcontainers.GCloud` (Google Cloud)
- `Testcontainers.Consul`, `Testcontainers.Vault`

**Development Tools (10+ modules):**
- `Testcontainers.WebDriver` (Selenium)
- `Testcontainers.MockServer`, `Testcontainers.Neo4j`
- `Testcontainers.Keycloak`, `Testcontainers.Grafana`

#### Basic Module Usage Pattern

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
        // Start the container
        await _postgres.StartAsync();
    }

    public async Task DisposeAsync()
    {
        // Clean up the container
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task ConnectionTest()
    {
        // Get connection string - credentials auto-generated
        var connectionString = _postgres.GetConnectionString();
        // connectionString: "Host=localhost;Port=49153;Database=postgres;Username=postgres;Password=..."

        // Use with Npgsql
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand("SELECT 1", connection);
        var result = await command.ExecuteScalarAsync();

        Assert.Equal(1, result);
    }
}
```

#### Module Configuration with Builder Pattern

Modules use a fluent builder API for configuration:

**Level 1: Basic Configuration**

```csharp
var postgres = new PostgreSqlBuilder()
    .WithImage("postgres:16-alpine")
    .WithDatabase("myapp_test")
    .WithUsername("custom_user")
    .WithPassword("custom_pass")
    .Build();
```

**Level 2: Advanced Configuration**

```csharp
// PostgreSQL with init scripts
var postgres = new PostgreSqlBuilder()
    .WithImage("postgres:16-alpine")
    .WithDatabase("myapp_test")
    .WithResourceMapping("./init.sql", "/docker-entrypoint-initdb.d/init.sql")
    .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(5432)))
    .Build();

// Redis with custom configuration
var redis = new RedisBuilder()
    .WithImage("redis:7-alpine")
    .WithCommand("redis-server", "--maxmemory", "256mb")
    .Build();

// Kafka with custom configuration
var kafka = new KafkaBuilder()
    .WithImage("confluentinc/confluent-local:7.5.0")
    .Build();
```

**Level 3: Network and Environment Configuration**

```csharp
var postgres = new PostgreSqlBuilder()
    .WithImage("postgres:16-alpine")
    .WithEnvironment("POSTGRES_INITDB_ARGS", "-E UTF8")
    .WithPortBinding(5432, 5432) // Optional: fixed port (not recommended for CI)
    .WithBindMount("/host/path", "/container/path")
    .WithTmpfsMount("/tmp")
    .WithLabel("test", "integration")
    .Build();
```

#### Module-Specific Helper Methods

Most modules provide convenience methods:

```csharp
// PostgreSQL: Get connection string
var postgres = new PostgreSqlBuilder().Build();
await postgres.StartAsync();
var connStr = postgres.GetConnectionString();

// SQL Server: Get connection string
var mssql = new MsSqlBuilder().Build();
await mssql.StartAsync();
var connStr = mssql.GetConnectionString();

// MongoDB: Get connection string
var mongo = new MongoDbBuilder().Build();
await mongo.StartAsync();
var connStr = mongo.GetConnectionString();

// Redis: Get connection string
var redis = new RedisBuilder().Build();
await redis.StartAsync();
var connStr = redis.GetConnectionString();

// Kafka: Get bootstrap address
var kafka = new KafkaBuilder().Build();
await kafka.StartAsync();
var bootstrapAddress = kafka.GetBootstrapAddress();

// Elasticsearch: Get connection string
var elasticsearch = new ElasticsearchBuilder().Build();
await elasticsearch.StartAsync();
var connStr = elasticsearch.GetConnectionString();
```

#### Finding the Right Module

1. **Browse available modules**: https://testcontainers.com/modules/?language=dotnet (complete, up-to-date list)
2. **Browse NuGet packages**: Search for "Testcontainers." on [NuGet.org](https://www.nuget.org/packages?q=testcontainers)
3. **Official documentation**: https://dotnet.testcontainers.org/
4. **GitHub repository**: https://github.com/testcontainers/testcontainers-dotnet
5. **Module examples**: Each module has examples in the repository

**Module naming pattern:**
```
Testcontainers.<ServiceName>
```

---

### 3. Using Generic Containers (Fallback)

When no pre-configured module exists, use generic containers with `ContainerBuilder`.

**IMPORTANT: Always add a wait strategy** to ensure the container is ready before tests run. This is critical for reliability, especially in CI environments.

```csharp
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

public class CustomContainerTests : IAsyncLifetime
{
    private readonly IContainer _container = new ContainerBuilder()
        .WithImage("custom-image:latest")
        .WithPortBinding(8080, true) // Random host port
        .WithEnvironment("APP_ENV", "test")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(8080).ForPath("/")))
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    [Fact]
    public void GetEndpoint()
    {
        // Get the mapped port
        var port = _container.GetMappedPublicPort(8080);
        var hostname = _container.Hostname;
        var endpoint = $"http://{hostname}:{port}";

        Assert.NotEmpty(endpoint);
    }
}
```

**Common generic container options:**

```csharp
var container = new ContainerBuilder()
    .WithImage("image:tag")

    // Ports
    .WithPortBinding(80, true)          // Random host port
    .WithPortBinding(443, 8443)         // Fixed host port (not recommended for CI)
    .WithExposedPort(80)                // Expose without binding

    // Environment
    .WithEnvironment("KEY", "value")
    .WithEnvironment(new Dictionary<string, string>
    {
        ["DATABASE_URL"] = "postgres://localhost/db",
        ["LOG_LEVEL"] = "debug"
    })

    // Files and Mounts
    .WithResourceMapping("./config.yml", "/app/config.yml")
    .WithBindMount("/host/path", "/container/path")
    .WithBindMount("/host/path", "/container/path", AccessMode.ReadOnly)
    .WithTmpfsMount("/tmp")

    // Wait strategies (REQUIRED for reliability)
    .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(80).ForPath("/")))
    // Or: .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(80)))
    // Or: .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("ready"))

    // Commands
    .WithCommand("arg1", "arg2")
    .WithEntrypoint("/bin/sh", "-c")

    // Labels
    .WithLabel("app", "myapp")
    .WithLabel("environment", "test")

    // Cleanup
    .WithCleanUp(true)  // Auto-cleanup (default: true)

    .Build();
```

---

### 4. Writing Integration Tests

#### Test Framework Integration

**xUnit (Recommended Pattern with IAsyncLifetime)**

```csharp
using Testcontainers.PostgreSql;
using Xunit;

public class DatabaseTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    // Called before each test
    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    // Called after each test
    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task CanConnectToDatabase()
    {
        var connectionString = _postgres.GetConnectionString();

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        Assert.NotNull(connection);
    }
}
```

**xUnit with Class Fixture (Shared Container)**

```csharp
using Testcontainers.PostgreSql;
using Xunit;

// Fixture: Container shared across multiple tests in the class
public class DatabaseFixture : IAsyncLifetime
{
    public PostgreSqlContainer Postgres { get; } = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await Postgres.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await Postgres.DisposeAsync();
    }
}

// Test class using the fixture
public class DatabaseTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public DatabaseTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void CanGetConnectionString()
    {
        var connectionString = _fixture.Postgres.GetConnectionString();
        Assert.NotEmpty(connectionString);
    }
}
```

**NUnit**

```csharp
using Testcontainers.PostgreSql;
using NUnit.Framework;

[TestFixture]
public class DatabaseTests
{
    private PostgreSqlContainer _postgres;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .Build();

        await _postgres.StartAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _postgres.DisposeAsync();
    }

    [Test]
    public async Task CanConnectToDatabase()
    {
        var connectionString = _postgres.GetConnectionString();

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        Assert.That(connection, Is.Not.Null);
    }
}
```

**MSTest**

```csharp
using Testcontainers.PostgreSql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class DatabaseTests
{
    private static PostgreSqlContainer _postgres;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .Build();

        await _postgres.StartAsync();
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        await _postgres.DisposeAsync();
    }

    [TestMethod]
    public async Task CanConnectToDatabase()
    {
        var connectionString = _postgres.GetConnectionString();

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        Assert.IsNotNull(connection);
    }
}
```

#### Theory/Parameterized Tests

**xUnit Theory:**

```csharp
public class VersionTests : IAsyncLifetime
{
    private PostgreSqlContainer _postgres;

    public async Task InitializeAsync()
    {
        // Will be called before each test
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (_postgres != null)
        {
            await _postgres.DisposeAsync();
        }
    }

    [Theory]
    [InlineData("postgres:14-alpine")]
    [InlineData("postgres:15-alpine")]
    [InlineData("postgres:16-alpine")]
    public async Task TestMultipleVersions(string image)
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage(image)
            .Build();

        await _postgres.StartAsync();

        var connectionString = _postgres.GetConnectionString();
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        Assert.NotNull(connection);
    }
}
```

---

### 5. Container Networking

#### Connecting Multiple Containers

```csharp
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;

public class MultiContainerTests : IAsyncLifetime
{
    private INetwork _network;
    private PostgreSqlContainer _postgres;
    private IContainer _app;

    public async Task InitializeAsync()
    {
        // Create custom network
        _network = new NetworkBuilder()
            .Build();

        await _network.CreateAsync();

        // Start database on network
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithNetwork(_network)
            .WithNetworkAliases("database")
            .Build();

        await _postgres.StartAsync();

        // Start application on same network
        _app = new ContainerBuilder()
            .WithImage("myapp:latest")
            .WithNetwork(_network)
            .WithNetworkAliases("app")
            .WithEnvironment("DB_HOST", "database")      // Use network alias
            .WithEnvironment("DB_PORT", "5432")          // Internal port
            .WithPortBinding(8080, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(8080).ForPath("/")))
            .Build();

        await _app.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _app.DisposeAsync();
        await _postgres.DisposeAsync();
        await _network.DeleteAsync();
    }

    [Fact]
    public void ApplicationCanCommunicateWithDatabase()
    {
        var appEndpoint = $"http://{_app.Hostname}:{_app.GetMappedPublicPort(8080)}";
        Assert.NotEmpty(appEndpoint);
    }
}
```

#### Accessing Container Ports

```csharp
[Fact]
public void GetPortInformation()
{
    // Method 1: Get mapped public port
    var publicPort = _container.GetMappedPublicPort(80);
    // publicPort = 49153 (random port assigned by Docker)

    // Method 2: Get hostname
    var hostname = _container.Hostname;
    // hostname = "localhost" (or docker host)

    // Method 3: Build full endpoint
    var endpoint = $"http://{_container.Hostname}:{_container.GetMappedPublicPort(80)}";
    // endpoint = "http://localhost:49153"
}
```

---

### 6. Resource Management & Cleanup

#### Cleanup Patterns

**Pattern 1: IAsyncLifetime (xUnit - Recommended)**

```csharp
public class DatabaseTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder().Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        // Container automatically cleaned up
    }
}
```

**Pattern 2: IAsyncDisposable**

```csharp
[Fact]
public async Task TestWithDisposable()
{
    await using var postgres = new PostgreSqlBuilder().Build();
    await postgres.StartAsync();

    // Use container...

    // Automatically disposed at end of scope
}
```

**Pattern 3: Explicit Cleanup**

```csharp
[Fact]
public async Task TestWithExplicitCleanup()
{
    var postgres = new PostgreSqlBuilder().Build();

    try
    {
        await postgres.StartAsync();

        // Use container...
    }
    finally
    {
        await postgres.DisposeAsync();
    }
}
```

#### Automatic Cleanup with Ryuk

Testcontainers for .NET uses **Ryuk**, a garbage collector that automatically cleans up containers even if tests crash or timeout:

- Runs as a sidecar container (`testcontainers/ryuk:0.13.0`)
- Monitors test session lifecycle
- Cleans up containers when session ends
- Handles parallel test execution

**Control Ryuk behavior:**

```csharp
// Disable Ryuk (not recommended)
Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED", "true");

// Custom Ryuk image
Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_CONTAINER_IMAGE", "testcontainers/ryuk:0.13.0");
```

**Cleanup options:**

```csharp
var container = new ContainerBuilder()
    .WithImage("nginx:alpine")
    .WithCleanUp(true)  // Enable auto-cleanup (default: true)
    .Build();
```

---

### 7. Configuration Patterns

#### Environment Variables

```csharp
var container = new ContainerBuilder()
    .WithImage("myapp:latest")
    .WithEnvironment("DATABASE_URL", "postgres://localhost/db")
    .WithEnvironment("LOG_LEVEL", "debug")
    .WithEnvironment("API_KEY", "test-key")
    .Build();

// Or with dictionary
var container = new ContainerBuilder()
    .WithImage("myapp:latest")
    .WithEnvironment(new Dictionary<string, string>
    {
        ["DATABASE_URL"] = "postgres://localhost/db",
        ["LOG_LEVEL"] = "debug"
    })
    .Build();
```

#### Executing Commands in Containers

```csharp
[Fact]
public async Task ExecuteCommandInContainer()
{
    await using var container = new ContainerBuilder()
        .WithImage("alpine:latest")
        .WithCommand("tail", "-f", "/dev/null")  // Keep container running
        .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("echo", "ready"))
        .Build();

    await container.StartAsync();

    // Execute command
    var execResult = await container.ExecAsync(new[] { "echo", "Hello, World!" });

    Assert.Equal(0, execResult.ExitCode);
    Assert.Contains("Hello, World!", execResult.Stdout);
}
```

#### Reading Logs

```csharp
[Fact]
public async Task ReadContainerLogs()
{
    await using var container = new ContainerBuilder()
        .WithImage("nginx:alpine")
        .WithPortBinding(80, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(80).ForPath("/")))
        .Build();

    await container.StartAsync();

    // Read logs
    var (stdout, stderr) = await container.GetLogsAsync();

    Assert.NotEmpty(stdout);
}
```

#### Files and Directories

```csharp
// Copy file to container
var container = new ContainerBuilder()
    .WithImage("nginx:alpine")
    .WithResourceMapping("./nginx.conf", "/etc/nginx/nginx.conf")
    .Build();

// Copy multiple files
var container = new ContainerBuilder()
    .WithImage("myapp:latest")
    .WithResourceMapping("./config.yml", "/app/config.yml")
    .WithResourceMapping("./secrets.json", "/app/secrets.json")
    .Build();

// Bind mount
var container = new ContainerBuilder()
    .WithImage("postgres:16")
    .WithBindMount("/host/data", "/var/lib/postgresql/data")
    .Build();

// Read-only bind mount
var container = new ContainerBuilder()
    .WithImage("myapp:latest")
    .WithBindMount("/host/config", "/app/config", AccessMode.ReadOnly)
    .Build();

// Copy file from container
await container.StartAsync();
var fileContent = await container.ReadFileAsync("/etc/nginx/nginx.conf");
```

#### Volume Mounts

```csharp
using DotNet.Testcontainers.Volumes;

public class VolumeTests : IAsyncLifetime
{
    private IVolume _volume;
    private IContainer _container;

    public async Task InitializeAsync()
    {
        // Create volume
        _volume = new VolumeBuilder()
            .Build();

        await _volume.CreateAsync();

        // Use volume in container
        _container = new ContainerBuilder()
            .WithImage("postgres:16")
            .WithVolumeMount(_volume, "/var/lib/postgresql/data")
            .Build();

        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
        await _volume.DeleteAsync();
    }
}
```

#### Temporary Filesystems

```csharp
var container = new ContainerBuilder()
    .WithImage("myapp:latest")
    .WithTmpfsMount("/tmp")
    .WithTmpfsMount("/app/temp")
    .Build();
```

---

### 8. Wait Strategies

**Wait strategies are critical for reliable tests.** They ensure containers are fully ready before tests run, which is especially important in CI environments where timing can vary.

**Best Practices:**
- ✅ **Always use wait strategies for services** - Ensures reliability
- ✅ **Choose appropriate wait strategies** based on your service
- ❌ **Never use `Task.Delay()` or `Thread.Sleep()`** - This is an anti-pattern that leads to flaky tests
- ✅ **Set reasonable timeouts** to handle slow CI environments

#### HTTP-Based Waiting (Recommended for Web Services)

```csharp
using DotNet.Testcontainers.Configurations;

var container = new ContainerBuilder()
    .WithImage("nginx:alpine")
    .WithPortBinding(80, true)
    .WithWaitStrategy(Wait.ForUnixContainer()
        .UntilHttpRequestIsSucceeded(r => r.ForPort(80).ForPath("/")))
    .Build();
```

#### Log-Based Waiting

```csharp
var container = new ContainerBuilder()
    .WithImage("elasticsearch:8.7.0")
    .WithWaitStrategy(Wait.ForUnixContainer()
        .UntilMessageIsLogged("started"))
    .Build();

// Wait for specific log message with timeout
var container = new ContainerBuilder()
    .WithImage("myapp:latest")
    .WithWaitStrategy(Wait.ForUnixContainer()
        .UntilMessageIsLogged("Application started")
        .WithTimeout(TimeSpan.FromMinutes(2)))
    .Build();
```

#### HTTP-Based Waiting

```csharp
var container = new ContainerBuilder()
    .WithImage("myapp:latest")
    .WithPortBinding(8080, true)
    .WithWaitStrategy(Wait.ForUnixContainer()
        .UntilHttpRequestIsSucceeded(request => request
            .ForPort(8080)
            .ForPath("/health")
            .ForStatusCode(HttpStatusCode.OK)))
    .Build();
```

#### Command-Based Waiting

```csharp
var container = new ContainerBuilder()
    .WithImage("postgres:16")
    .WithWaitStrategy(Wait.ForUnixContainer()
        .UntilCommandIsCompleted("pg_isready"))
    .Build();
```

#### Multiple Wait Strategies

```csharp
var container = new ContainerBuilder()
    .WithImage("myapp:latest")
    .WithPortBinding(8080, true)
    .WithWaitStrategy(Wait.ForUnixContainer()
        .UntilHttpRequestIsSucceeded(r => r.ForPort(8080).ForPath("/"))
        .UntilMessageIsLogged("Application started")
        .UntilHttpRequestIsSucceeded(r => r.ForPort(8080).ForPath("/health")))
    .Build();
```

#### Custom Wait Strategies

```csharp
var container = new ContainerBuilder()
    .WithImage("myapp:latest")
    .WithWaitStrategy(Wait.ForUnixContainer()
        .AddCustomWaitStrategy(new MyCustomWaitStrategy()))
    .Build();

public class MyCustomWaitStrategy : IWaitUntil
{
    public async Task<bool> UntilAsync(IContainer container)
    {
        // Custom wait logic
        return true;
    }
}
```

---

### 9. Troubleshooting

#### Check Docker Availability

```csharp
[Fact]
public void CheckDockerConnection()
{
    var dockerEndpoint = TestcontainersSettings.OS.DockerEndpointAuthConfig;
    Assert.NotNull(dockerEndpoint);
}
```

#### Debug Container Logs

```csharp
[Fact]
public async Task DebugWithLogging()
{
    await using var container = new ContainerBuilder()
        .WithImage("myapp:latest")
        .WithPortBinding(8080, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(8080).ForPath("/")))
        .Build();

    await container.StartAsync();

    // Read logs
    var (stdout, stderr) = await container.GetLogsAsync();
    _output.WriteLine($"STDOUT:\n{stdout}");
    _output.WriteLine($"STDERR:\n{stderr}");

    // Verify container is running
    _output.WriteLine($"Container ID: {container.Id}");
}
```

#### Common Issues

**Issue: Container startup timeout**

```csharp
// Increase wait timeout
var container = new ContainerBuilder()
    .WithImage("slow-starting-app:latest")
    .WithWaitStrategy(Wait.ForUnixContainer()
        .UntilHttpRequestIsSucceeded(r => r.ForPort(8080).ForPath("/"))
        .WithTimeout(TimeSpan.FromMinutes(5)))  // Increase timeout
    .Build();
```

**Issue: Port already in use**
- Testcontainers auto-assigns random ports when using `.WithPortBinding(port, true)`
- Avoid fixed port bindings unless necessary
- Check for leaked containers: `docker ps -a`

**Issue: Image pull failures**

```bash
# Pull manually first to verify
docker pull postgres:16

# For private registries, login first
docker login registry.example.com
# Testcontainers will use credentials from Docker config
```

**Issue: Container not cleaning up**

```csharp
// Verify cleanup is enabled
var container = new ContainerBuilder()
    .WithImage("nginx:alpine")
    .WithCleanUp(true)  // Ensure auto-cleanup is enabled (default: true)
    .Build();

// Check Ryuk is running
// docker ps | grep ryuk
```

#### Environment Variables for Configuration

```csharp
// Custom Docker host
Environment.SetEnvironmentVariable("DOCKER_HOST", "tcp://localhost:2375");

// Disable Ryuk (not recommended)
Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED", "true");

// Custom Ryuk image
Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_CONTAINER_IMAGE", "testcontainers/ryuk:0.13.0");

// Hub image name prefix (for private registries)
Environment.SetEnvironmentVariable("TESTCONTAINERS_HUB_IMAGE_NAME_PREFIX", "my.registry.com/");
```

---

## Examples

### Example 1: PostgreSQL Integration Test

```csharp
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

public class UserRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("testdb")
        .WithUsername("testuser")
        .WithPassword("testpass")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // Initialize schema
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(@"
            CREATE TABLE users (
                id SERIAL PRIMARY KEY,
                name TEXT NOT NULL,
                email TEXT UNIQUE NOT NULL,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            )", connection);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task CreateUser_ShouldInsertUser()
    {
        // Arrange
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync();

        // Act
        await using var command = new NpgsqlCommand(
            "INSERT INTO users (name, email) VALUES (@name, @email) RETURNING id",
            connection);

        command.Parameters.AddWithValue("name", "Alice");
        command.Parameters.AddWithValue("email", "alice@example.com");

        var userId = await command.ExecuteScalarAsync();

        // Assert
        Assert.NotNull(userId);
    }

    [Fact]
    public async Task GetUser_ShouldReturnUser()
    {
        // Arrange
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync();

        await using var insertCmd = new NpgsqlCommand(
            "INSERT INTO users (name, email) VALUES (@name, @email)",
            connection);
        insertCmd.Parameters.AddWithValue("name", "Bob");
        insertCmd.Parameters.AddWithValue("email", "bob@example.com");
        await insertCmd.ExecuteNonQueryAsync();

        // Act
        await using var selectCmd = new NpgsqlCommand(
            "SELECT name, email FROM users WHERE email = @email",
            connection);
        selectCmd.Parameters.AddWithValue("email", "bob@example.com");

        await using var reader = await selectCmd.ExecuteReaderAsync();
        await reader.ReadAsync();

        var name = reader.GetString(0);
        var email = reader.GetString(1);

        // Assert
        Assert.Equal("Bob", name);
        Assert.Equal("bob@example.com", email);
    }
}
```

### Example 2: Redis Cache Test

```csharp
using StackExchange.Redis;
using Testcontainers.Redis;
using Xunit;

public class RedisCacheTests : IAsyncLifetime
{
    private readonly RedisContainer _redis = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    private IConnectionMultiplexer _connection;
    private IDatabase _db;

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
        // Act
        await _db.StringSetAsync("key1", "value1");
        var value = await _db.StringGetAsync("key1");

        // Assert
        Assert.Equal("value1", value);
    }

    [Fact]
    public async Task SetWithExpiration_ShouldExpireKey()
    {
        // Act
        await _db.StringSetAsync("key2", "value2", TimeSpan.FromSeconds(1));
        var valueBefore = await _db.StringGetAsync("key2");

        await Task.Delay(TimeSpan.FromSeconds(2));

        var valueAfter = await _db.StringGetAsync("key2");

        // Assert
        Assert.Equal("value2", valueBefore);
        Assert.True(valueAfter.IsNull);
    }
}
```

### Example 3: SQL Server with Entity Framework Core

```csharp
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using Xunit;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

public class EntityFrameworkTests : IAsyncLifetime
{
    private readonly MsSqlContainer _mssql = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    private ApplicationDbContext _dbContext;

    public async Task InitializeAsync()
    {
        await _mssql.StartAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_mssql.GetConnectionString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _mssql.DisposeAsync();
    }

    [Fact]
    public async Task AddUser_ShouldPersistToDatabase()
    {
        // Arrange
        var user = new User
        {
            Name = "Alice",
            Email = "alice@example.com"
        };

        // Act
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == "alice@example.com");

        Assert.NotNull(savedUser);
        Assert.Equal("Alice", savedUser.Name);
    }
}
```

### Example 4: Kafka Producer/Consumer Test

```csharp
using Confluent.Kafka;
using Testcontainers.Kafka;
using Xunit;

public class KafkaTests : IAsyncLifetime
{
    private readonly KafkaContainer _kafka = new KafkaBuilder()
        .WithImage("confluentinc/confluent-local:7.5.0")
        .Build();

    public async Task InitializeAsync()
    {
        await _kafka.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _kafka.DisposeAsync();
    }

    [Fact]
    public async Task ProduceAndConsume_ShouldTransferMessage()
    {
        // Arrange
        var topic = "test-topic";
        var bootstrapServers = _kafka.GetBootstrapAddress();

        // Producer
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };

        using var producer = new ProducerBuilder<string, string>(producerConfig).Build();

        // Consumer
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = "test-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        consumer.Subscribe(topic);

        // Act
        await producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = "key1",
            Value = "Hello, Kafka!"
        });

        var consumeResult = consumer.Consume(TimeSpan.FromSeconds(10));

        // Assert
        Assert.NotNull(consumeResult);
        Assert.Equal("Hello, Kafka!", consumeResult.Message.Value);
    }
}
```

### Example 5: Multi-Container Application Stack

```csharp
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;

public class FullStackTests : IAsyncLifetime
{
    private INetwork _network;
    private PostgreSqlContainer _postgres;
    private RedisContainer _redis;
    private IContainer _app;

    public async Task InitializeAsync()
    {
        // Create network
        _network = new NetworkBuilder().Build();
        await _network.CreateAsync();

        // Start PostgreSQL
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithNetwork(_network)
            .WithNetworkAliases("database")
            .Build();

        await _postgres.StartAsync();

        // Start Redis
        _redis = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithNetwork(_network)
            .WithNetworkAliases("cache")
            .Build();

        await _redis.StartAsync();

        // Start application
        _app = new ContainerBuilder()
            .WithImage("myapp:latest")
            .WithNetwork(_network)
            .WithNetworkAliases("app")
            .WithEnvironment("DB_HOST", "database")
            .WithEnvironment("DB_PORT", "5432")
            .WithEnvironment("REDIS_HOST", "cache")
            .WithEnvironment("REDIS_PORT", "6379")
            .WithPortBinding(8080, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(8080).ForPath("/")))
            .Build();

        await _app.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _app.DisposeAsync();
        await _redis.DisposeAsync();
        await _postgres.DisposeAsync();
        await _network.DeleteAsync();
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnOk()
    {
        // Arrange
        var endpoint = $"http://{_app.Hostname}:{_app.GetMappedPublicPort(8080)}";

        using var httpClient = new HttpClient();

        // Act
        var response = await httpClient.GetAsync($"{endpoint}/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
```

---

## Best Practices

1. **Always use pre-configured modules when available** - They provide sensible defaults and helper methods
2. **Use async lifecycle management** - Proper async initialization and cleanup (IAsyncLifetime in xUnit, [OneTimeSetUp]/[OneTimeTearDown] in NUnit, [ClassInitialize]/[ClassCleanup] in MSTest)
3. **Always add wait strategies** - Ensures containers are ready before tests run; never use `Task.Delay()`
4. **Choose appropriate wait strategies** - Use HTTP for health endpoints, logs for startup messages, or ports for availability
5. **Test against multiple configurations** - Use parameterized tests to validate against different versions or configurations (Theory/InlineData in xUnit, TestCase in NUnit, DataRow in MSTest)
6. **Use custom networks** - For multi-container communication
7. **Keep containers ephemeral** - Don't rely on state between tests
8. **Share containers when appropriate** - Use fixtures or setup methods to share containers across tests for better performance
9. **Use module helper methods** - E.g., `GetConnectionString()`, `GetBootstrapAddress()`
10. **Debug with logs** - Use `GetLogsAsync()` when troubleshooting
11. **Use builder pattern** - Fluent API for clear, maintainable configuration

---

## Additional Resources

- **Official Documentation**: https://dotnet.testcontainers.org/
- **NuGet Packages**: https://www.nuget.org/packages?q=testcontainers
- **GitHub Repository**: https://github.com/testcontainers/testcontainers-dotnet
- **Examples**: https://github.com/testcontainers/testcontainers-dotnet/tree/develop/examples
- **Community Slack**: [testcontainers.slack.com](https://testcontainers.slack.com)
