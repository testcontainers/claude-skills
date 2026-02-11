---
name: testcontainers-dotnet
description: A comprehensive guide for using Testcontainers for .NET (4.10.0+) to write reliable integration tests with Docker containers in .NET projects. Supports 65+ pre-configured modules for databases, message queues, cloud services, and more.
applies_to: Testcontainers for .NET 4.10.0+
license: MIT
---

# Testcontainers for .NET Integration Testing

A comprehensive guide for using Testcontainers for .NET (4.10.0+) to write reliable integration tests with Docker containers in .NET projects.

## Description

This skill helps you write integration tests using Testcontainers for .NET, a .NET library that provides lightweight, throwaway instances of common databases, message queues, web browsers, or anything that can run in a Docker container.

**Key capabilities**:
- Use 65+ pre-configured modules for common services (databases, message queues, cloud services, etc.)
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

## Decision Rule (Recommended)

1. **If a pre-configured module exists for your service**, use the module.
2. **If no module exists**, use a generic container with `ContainerBuilder` **and always define an explicit wait strategy**.

## Prerequisites

- **Docker or Podman** installed and running
- **.NET 8.0+** (check project requirements; library supports .NET and .NET Standard)
- **Docker socket** accessible at standard locations (Docker Desktop on macOS/Windows, `/var/run/docker.sock` on Linux)
- **Test framework**: xUnit, NUnit, or MSTest

## Instructions

### Quick Start (Framework-Agnostic)

Use this when you want a minimal, reliable integration test setup without committing to xUnit/NUnit/MSTest patterns up front.

1. Create (or choose) a test project using your preferred test framework.
2. Add the Testcontainers module for your service (prefer modules over generic containers).
3. Add the client library for the service you will connect to.
4. Write a single test that:
    - starts the container
    - connects to the service and performs a small smoke operation
    - disposes the container (for example, via `await using`)
5. Run the test project.

```bash
# Example: PostgreSQL (module + client library)
dotnet add package Testcontainers.PostgreSql
dotnet add package Npgsql

# Run your tests (command varies by runner, but this works for most)
dotnet test
```

```csharp
// NuGet dependencies:
// - dotnet add package Npgsql
// - dotnet add package Testcontainers.PostgreSql
// - dotnet add package xunit.v3

using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Testcontainers.PostgreSql;

public sealed class PostgresSmokeTest
{
    // Note: Add your framework's test attribute (e.g., [Fact]/[Test]/[TestMethod]).
    public async Task CanQueryPostgres()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();
        await postgres.StartAsync(CancellationToken.None);

        await using var connection = new NpgsqlConnection(postgres.GetConnectionString());
        await connection.OpenAsync(CancellationToken.None);

        await using var command = new NpgsqlCommand("SELECT 1", connection);
        var result = await command.ExecuteScalarAsync(CancellationToken.None);

        if (!Equals(result, 1))
        {
            throw new InvalidOperationException($"Expected SELECT 1 to return 1. Actual: {result}");
        }
    }
}
```

### What I Need From You (Checklist)

When you ask for help, include these details so the generated test code matches your environment:

- **Test framework**: xUnit / NUnit / MSTest (and version if relevant)
- **.NET version**: e.g., net8.0
- **Container runtime**: Docker Desktop / Docker Engine / Podman (and OS)
- **Service under test**: PostgreSQL / Redis / SQL Server / Kafka / ...
- **Image + tag**: e.g., `postgres:16-alpine` (pin versions for CI stability)
- **How the test should connect**: host port mapping vs container-to-container network
- **Readiness signal**: HTTP endpoint, log line, command (if you know it)
- **Data setup**: schema/init scripts, seed data, migrations (and where they live)
- **Lifecycle**: per-test container vs shared fixture (performance vs isolation)
- **Parallelism/CI**: Will tests run in parallel? Any CI constraints/timeouts?

If you do not know an item, say so (the default recommendation is: module + explicit wait strategy + random host ports + dispose in teardown).

### 1. Installation & Setup

Add Testcontainers for .NET to your test project:

```bash
# Core library (required)
# For modules, the core library will be resolved through transitive dependencies.
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

**Verify Docker availability**:

Note: Testcontainers usually fails (throws) when **creating/starting containers or other resources** if no container runtime is reachable. The snippet below is a lightweight sanity check that helps you see what configuration Testcontainers resolves on the current machine.

```csharp
using DotNet.Testcontainers.Configurations;
using Xunit;

[Fact]
public void DockerIsAvailable()
{
    var testcontainersConfiguration = TestcontainersSettings.OS;
    Assert.NotNull(testcontainersConfiguration);
}
```

---

### 2. Using Pre-Configured Modules (Recommended Approach)

**Testcontainers for .NET provides 65+ pre-configured modules** that offer production-ready configurations, sensible defaults, and helper methods. **Always prefer modules over generic containers** when available.

#### Why Use Modules?

- **Sensible defaults**: Pre-configured ports, environment variables, and wait strategies
- **Connection helpers**: Built-in properties like `GetConnectionString()`, `GetBootstrapAddress()`
- **Specialized features**: Module-specific functionality (e.g., running scripts inside the container)
- **Automatic credentials**: Secure credential generation and management
- **Battle-tested**: Used in production by thousands of projects

#### Available Module Categories

**Databases (15+ modules)**:
- `Testcontainers.Cassandra`
- `Testcontainers.ClickHouse`
- `Testcontainers.CosmosDb`
- `Testcontainers.CouchDb`
- `Testcontainers.Db2`
- `Testcontainers.DynamoDb`
- `Testcontainers.InfluxDb`
- `Testcontainers.MariaDb`
- `Testcontainers.MongoDB`
- `Testcontainers.MsSql`
- `Testcontainers.MySql`
- `Testcontainers.Oracle`
- `Testcontainers.PostgreSql`
- `Testcontainers.Redis`

**Message Queues (5+ modules)**:
- `Testcontainers.Kafka`
- `Testcontainers.NATS`
- `Testcontainers.Pulsar`
- `Testcontainers.RabbitMq`
- `Testcontainers.Redpanda`

**Search & Storage (5+ modules)**:
- `Testcontainers.Azurite`
- `Testcontainers.Elasticsearch`
- `Testcontainers.LocalStack`
- `Testcontainers.Minio`
- `Testcontainers.Qdrant`

**Cloud & Infrastructure (5+ modules)**:
- `Testcontainers.Azurite` (Azure Storage)
- `Testcontainers.GCloud` (Google Cloud)
- `Testcontainers.LocalStack` (AWS services)
- `Testcontainers.Consul`
- `Testcontainers.Vault`

**Development Tools (10+ modules)**:
- `Testcontainers.WebDriver` (Selenium)
- `Testcontainers.Grafana`
- `Testcontainers.Keycloak`
- `Testcontainers.MockServer`
- `Testcontainers.Neo4j`

#### Basic Module Usage Pattern

```csharp
// NuGet dependencies:
// - dotnet add package Npgsql
// - dotnet add package Testcontainers.PostgreSql
// - dotnet add package xunit.v3

using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

public sealed class DatabaseTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task ConnectionTest()
    {
        // Includes mapped port and generated credentials.
        var connectionString = _postgres.GetConnectionString();

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        await using var command = new NpgsqlCommand("SELECT 1", connection);
        var result = await command.ExecuteScalarAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, result);
    }
}
```

#### Module Configuration with Builder Pattern

Modules use a fluent builder API for configuration:

**Level 1: Basic Configuration**

```csharp
var postgres = new PostgreSqlBuilder("postgres:16-alpine")
    .WithDatabase("myapp_test")
    .WithUsername("custom_user")
    .WithPassword("custom_pass")
    .Build();
```

**Level 2: Advanced Configuration**

```csharp
// PostgreSQL with init scripts
var postgres = new PostgreSqlBuilder("postgres:16-alpine")
    .WithDatabase("myapp_test")
    .WithResourceMapping("./init.sql", "/docker-entrypoint-initdb.d/init.sql")
    .Build();

// Redis with custom configuration
var redis = new RedisBuilder("redis:7-alpine")
    .WithCommand("redis-server", "--maxmemory", "256mb")
    .Build();

// Kafka with custom configuration
var kafka = new KafkaBuilder("confluentinc/cp-kafka:7.5.12")
    .WithKRaft()
    .Build();
```

**Level 3: Network and Environment Configuration**

```csharp
var postgres = new PostgreSqlBuilder("postgres:16-alpine")
    .WithEnvironment("POSTGRES_INITDB_ARGS", "-E UTF8")
    .WithLabel("environment", "test")
    .WithTmpfsMount("/tmp")
    .WithBindMount("/host/path", "/container/path") // Optional: mount directory or file (not recommended)
    .WithPortBinding(5432, 5432) // Optional: fixed port (not recommended)
    .Build();
```

#### Module-Specific Helper Methods

Most modules provide convenience methods:

```csharp
// PostgreSQL
await using var postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();
await postgres.StartAsync();
var postgresConnectionString = postgres.GetConnectionString();

// SQL Server
await using var mssql = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04").Build();
await mssql.StartAsync();
var mssqlConnectionString = mssql.GetConnectionString();

// MongoDB
await using var mongo = new MongoDbBuilder("mongo:6.0").Build();
await mongo.StartAsync();
var mongoConnectionString = mongo.GetConnectionString();

// Redis
await using var redis = new RedisBuilder("redis:7-alpine").Build();
await redis.StartAsync();
var redisConnectionString = redis.GetConnectionString();

// Kafka
await using var kafka = new KafkaBuilder("confluentinc/cp-kafka:7.5.12").Build();
await kafka.StartAsync();
var kafkaBootstrapAddress = kafka.GetBootstrapAddress();

// Elasticsearch
await using var elasticsearch = new ElasticsearchBuilder("elasticsearch:8.7.0").Build();
await elasticsearch.StartAsync();
var elasticsearchConnectionString = elasticsearch.GetConnectionString();
```

#### Finding the Right Module

1. **Browse available modules**: https://testcontainers.com/modules/?language=dotnet (complete, up-to-date list)
2. **Browse NuGet packages**: Search for "Testcontainers" on [NuGet.org](https://www.nuget.org/packages?q=testcontainers)
3. **Official documentation**: https://dotnet.testcontainers.org/
4. **GitHub repository**: https://github.com/testcontainers/testcontainers-dotnet
5. **Module examples**: Each module has examples/tests in the repository

**Module naming pattern**:

```
Testcontainers.<ServiceName>
```

---

### 3. Using Generic Containers (Fallback)

When no pre-configured module exists, use generic containers with `ContainerBuilder`.

**Important: Always add a wait strategy** to ensure the container is ready before tests run. This is critical for reliability, especially in CI environments.

```csharp
// NuGet dependencies:
// - dotnet add package Testcontainers
// - dotnet add package xunit.v3

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Xunit;

public sealed class CustomContainerTests : IAsyncLifetime
{
    private readonly IContainer _container = new ContainerBuilder("custom-image:latest")
        .WithPortBinding(8080, true) // Random host port (recommended)
        .WithEnvironment("APP_ENV", "test")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(8080).ForPath("/")))
        .Build();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    [Fact]
    public void GetEndpoint()
    {
        // Use mapped host port + resolved hostname.
        var port = _container.GetMappedPublicPort(8080);
        var hostname = _container.Hostname;

        // Prefer Hostname over hard-coding localhost (works across runtimes/CI).
        var endpoint = $"http://{hostname}:{port}";

        Assert.True(port > 0, $"Port value must be greater than 0. Actual value: '{port}'.");
    }
}
```

**Common generic container options**:

```csharp
var container = new ContainerBuilder("image:tag")

    // Ports
    .WithPortBinding(80, true)          // Random host port
    .WithPortBinding(443, 8443)         // Fixed port (not recommended)
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
    // Bind mounts are not recommended; prefer WithResourceMapping.
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

Note: The **xUnit.net examples in this document use xUnit.net v3** (for example `TestContext.Current.CancellationToken`). The overall patterns are **framework-agnostic**: the same container setup/teardown concepts apply to NUnit and MSTest, and you can adapt cancellation-token usage to your test framework/version.

**xUnit (Recommended Pattern with IAsyncLifetime)**

```csharp
// NuGet dependencies:
// - dotnet add package Npgsql
// - dotnet add package Testcontainers.PostgreSql
// - dotnet add package xunit.v3

using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

public sealed class DatabaseTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task CanConnectToDatabase()
    {
        var connectionString = _postgres.GetConnectionString();

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(connection);
    }
}
```

**xUnit with Class Fixture (Shared Container)**

```csharp
// NuGet dependencies:
// - dotnet add package Testcontainers.PostgreSql
// - dotnet add package xunit.v3

using Testcontainers.PostgreSql;
using Xunit;

// Fixture: Container shared across multiple tests in the class
public sealed class DatabaseFixture : IAsyncLifetime
{
    public PostgreSqlContainer Postgres { get; } = new PostgreSqlBuilder("postgres:16-alpine").Build();

    public async ValueTask InitializeAsync()
    {
        await Postgres.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await Postgres.DisposeAsync();
    }
}

// Test class using the fixture
public sealed class DatabaseTests : IClassFixture<DatabaseFixture>
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
// NuGet dependencies:
// - dotnet add package Npgsql
// - dotnet add package NUnit
// - dotnet add package Testcontainers.PostgreSql

using Npgsql;
using Testcontainers.PostgreSql;
using NUnit.Framework;

[TestFixture]
public sealed class DatabaseTests
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
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
// NuGet dependencies:
// - dotnet add package MSTest.TestFramework
// - dotnet add package Npgsql
// - dotnet add package Testcontainers.PostgreSql

using Npgsql;
using Testcontainers.PostgreSql;

[TestClass]
public sealed class DatabaseTests
{
    private static readonly PostgreSqlContainer Postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        await Postgres.StartAsync();
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        await Postgres.DisposeAsync();
    }

    [TestMethod]
    public async Task CanConnectToDatabase()
    {
        var connectionString = Postgres.GetConnectionString();

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        Assert.IsNotNull(connection);
    }
}
```

#### Theory/Parameterized Tests

**xUnit Theory**:

```csharp
// NuGet dependencies:
// - dotnet add package Npgsql
// - dotnet add package Testcontainers.PostgreSql
// - dotnet add package xunit.v3

using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

public sealed class VersionTests
{
    [Theory]
    [InlineData("postgres:14-alpine")]
    [InlineData("postgres:15-alpine")]
    [InlineData("postgres:16-alpine")]
    public async Task TestMultipleVersions(string image)
    {
        await using var postgres = new PostgreSqlBuilder(image).Build();

        await postgres.StartAsync(TestContext.Current.CancellationToken);

        var connectionString = postgres.GetConnectionString();

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(connection);
    }
}
```

---

### 5. Container Networking

#### Connecting Multiple Containers

```csharp
// NuGet dependencies:
// - dotnet add package Testcontainers.PostgreSql
// - dotnet add package xunit.v3

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Testcontainers.PostgreSql;
using Xunit;

public sealed class MultiContainerTests : IAsyncLifetime
{
    private INetwork _network;
    private PostgreSqlContainer _postgres;
    private IContainer _app;

    public async ValueTask InitializeAsync()
    {
        // Create custom network
        _network = new NetworkBuilder()
            .Build();

        // Start database on network
        _postgres = new PostgreSqlBuilder("postgres:16-alpine")
            .WithNetwork(_network)
            .WithNetworkAliases("database")
            .Build();

        // Start app on network
        _app = new ContainerBuilder("custom-image:latest")
            .WithNetwork(_network)
            .WithNetworkAliases("app")
            .WithEnvironment("DB_HOST", "database") // Use network alias to connect to the DB
            .WithEnvironment("DB_PORT", "5432") // Use internal DB port
            .WithPortBinding(8080, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(8080).ForPath("/")))
            .Build();

        await _network.CreateAsync();
        await _postgres.StartAsync();
        await _app.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _app.DisposeAsync();
        await _postgres.DisposeAsync();
        await _network.DeleteAsync();
    }

    [Fact]
    public void AppCanCommunicateWithDatabase()
    {
        var endpoint = $"http://{_app.Hostname}:{_app.GetMappedPublicPort(8080)}";
        Assert.NotEmpty(endpoint);
    }
}
```

#### Accessing Container Services

```csharp
[Fact]
public void GetServiceInformation()
{
    // Method 1: Get mapped public port
    var publicPort = _container.GetMappedPublicPort(80);
    // publicPort = 49153 (random port assigned by Docker)

    // Method 2: Get hostname
    var hostname = _container.Hostname;
    // hostname = "localhost" (or Docker host)

    // Method 3: Build full endpoint
    var endpoint = $"http://{_container.Hostname}:{_container.GetMappedPublicPort(80)}";
    // endpoint = "http://localhost:49153"
}
```

---

### 6. Resource Management & Cleanup

#### Cleanup Patterns

Goal: Start containers only for the time you need them, and ensure cleanup runs reliably even when tests fail.

**Pattern 1: IAsyncLifetime (xUnit - Recommended)**

```csharp
public sealed class DatabaseTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        // Ryuk cleans up automatically, but disposing early is still best practice.
        await _postgres.DisposeAsync();
    }
}
```

**Pattern 2: IAsyncDisposable**

```csharp
[Fact]
public async Task TestWithDisposable()
{
    await using var postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();
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
    var postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();

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

Testcontainers for .NET uses **[Ryuk](https://github.com/testcontainers/moby-ryuk)**, a garbage collector that automatically cleans up containers even if tests crash or timeout:

- Runs as a sidecar container (e.g., `testcontainers/ryuk:0.14.0`)
- Monitors test session lifecycle
- Cleans up containers when session ends
- Handles parallel test execution

**Control Ryuk behavior**:

```csharp
// Disable Ryuk (not recommended)
Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED", "true");

// Custom Ryuk image
Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_CONTAINER_IMAGE", "testcontainers/ryuk:0.14.0");
```

**Cleanup options**:

```csharp
var container = new ContainerBuilder("nginx:alpine")
    .WithCleanUp(true)  // Enable auto-cleanup (default: true)
    .Build();
```

---

### 7. Configuration Patterns

#### Environment Variables

```csharp
var container = new ContainerBuilder("custom-image:latest")
    .WithEnvironment("DATABASE_URL", "postgres://localhost/db")
    .WithEnvironment("LOG_LEVEL", "debug")
    .Build();

// Same idea, using a dictionary
var containerWithDictionary = new ContainerBuilder("custom-image:latest")
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
    await using var container = new ContainerBuilder("alpine:3.23")
        .WithCommand("tail", "-f", "/dev/null")  // Keep container running
        .Build();

    await container.StartAsync();

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
    await using var container = new ContainerBuilder("nginx:alpine")
        .WithPortBinding(80, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(80).ForPath("/")))
        .Build();

    await container.StartAsync();

    var (stdout, stderr) = await container.GetLogsAsync();

    Assert.NotEmpty(stdout);
}
```

#### Files and Directories

```csharp
// Copy a local file into the container
var nginx = new ContainerBuilder("nginx:alpine")
    .WithResourceMapping("./nginx.conf", "/etc/nginx/nginx.conf")
    .Build();

// Copy multiple files
var appWithFiles = new ContainerBuilder("custom-image:latest")
    .WithResourceMapping("./config.yml", "/app/config.yml")
    .WithResourceMapping("./secrets.json", "/app/secrets.json")
    .Build();

// Bind mount (not recommended for hermetic tests)
var postgresWithBindMount = new ContainerBuilder("postgres:16")
    .WithBindMount("/host/data", "/var/lib/postgresql/data")
    .Build();

// Read-only bind mount
var appWithReadOnlyMount = new ContainerBuilder("custom-image:latest")
    .WithBindMount("/host/config", "/app/config", AccessMode.ReadOnly)
    .Build();

// Read a file from a running container
await nginx.StartAsync();
var nginxConf = await nginx.ReadFileAsync("/etc/nginx/nginx.conf");
```

#### Volume Mounts

```csharp
public sealed class VolumeTests : IAsyncLifetime
{
    private IVolume _volume;
    private IContainer _container;

    public async ValueTask InitializeAsync()
    {
        // Create volume
        _volume = new VolumeBuilder()
            .Build();

        // Use volume in container
        _container = new ContainerBuilder("postgres:16-alpine")
            .WithVolumeMount(_volume, "/var/lib/postgresql/data")
            .Build();

        await _volume.CreateAsync();
        await _container.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
        await _volume.DeleteAsync();
    }
}
```

#### Temporary Filesystems

```csharp
var container = new ContainerBuilder("custom-image:latest")
    .WithTmpfsMount("/tmp")
    .WithTmpfsMount("/app/temp")
    .Build();
```

---

### 8. Wait Strategies

**Wait strategies are critical for reliable tests.** They ensure containers are fully ready before tests run, which is especially important in CI environments where timing can vary.

**Best Practices**:
- ✅ **Always use wait strategies for services** - Ensures reliability
- ✅ **Choose appropriate wait strategies** based on your service
- ❌ **Never use `Task.Delay()` or `Thread.Sleep()` as a readiness mechanism** - This is an anti-pattern that leads to flaky tests
- ✅ **Set reasonable timeouts** to handle slow CI environments

**Common pitfall**: A `Task.Delay(...)` can be fine *inside a test* (for example, waiting for an expiration to happen). The anti-pattern is using fixed sleeps/delays to decide **when a containerized service is ready**. For readiness, always prefer explicit wait strategies.

#### HTTP-Based Waiting (Recommended for Web Services)

```csharp
using System.Net;

var container = new ContainerBuilder("nginx:alpine")
    .WithPortBinding(80, true)
    .WithWaitStrategy(Wait.ForUnixContainer()
        .UntilHttpRequestIsSucceeded(r => r.ForPort(80).ForPath("/")))
    .Build();

// Wait for a specific path and expected status code
var healthCheckContainer = new ContainerBuilder("custom-image:latest")
    .WithPortBinding(8080, true)
    .WithWaitStrategy(Wait.ForUnixContainer()
        .UntilHttpRequestIsSucceeded(request => request
            .ForPort(8080)
            .ForPath("/health")
            .ForStatusCode(HttpStatusCode.OK)))
    .Build();
```

#### Log-Based Waiting

```csharp
var container = new ContainerBuilder("elasticsearch:8.7.0")
    .WithWaitStrategy(Wait.ForUnixContainer()
        .UntilMessageIsLogged("started"))
    .Build();

// Wait for specific log message with timeout
var containerWithTimeout = new ContainerBuilder("elasticsearch:8.7.0")
    .WithWaitStrategy(Wait.ForUnixContainer()
        .UntilMessageIsLogged("started", o => o.WithTimeout(TimeSpan.FromMinutes(5))))
    .Build();
```

#### Command-Based Waiting

```csharp
var container = new ContainerBuilder("postgres:16-alpine")
    .WithWaitStrategy(Wait.ForUnixContainer()
        .UntilCommandIsCompleted("pg_isready"))
    .Build();
```

#### Multiple Wait Strategies

```csharp
var container = new ContainerBuilder("custom-image:latest")
    .WithPortBinding(8080, true)
    .WithWaitStrategy(Wait.ForUnixContainer()
        .UntilHttpRequestIsSucceeded(r => r.ForPort(8080).ForPath("/"))
        .UntilMessageIsLogged("Application started")
        .UntilHttpRequestIsSucceeded(r => r.ForPort(8080).ForPath("/health")))
    .Build();
```

#### Custom Wait Strategies

```csharp
var container = new ContainerBuilder("custom-image:latest")
    .WithWaitStrategy(Wait.ForUnixContainer()
        .AddCustomWaitStrategy(new MyCustomWaitStrategy()))
    .Build();

public sealed class MyCustomWaitStrategy : IWaitUntil
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

#### Verify Docker Availability

```csharp
[Fact]
public void CheckDockerConnection()
{
    var dockerEndpoint = TestcontainersSettings.OS.DockerEndpointAuthConfig;
    Assert.NotNull(dockerEndpoint);
}
```

#### Debug Container Logs

Note: In xUnit, `_output` typically comes from `ITestOutputHelper` injected into the test class constructor. If you are using NUnit/MSTest (or you prefer a quick local repro), you can replace `_output.WriteLine(...)` with your framework's logging mechanism or `Console.WriteLine(...)`.

```csharp
[Fact]
public async Task DebugWithLogging()
{
    await using var container = new ContainerBuilder("custom-image:latest")
        .WithPortBinding(8080, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(8080).ForPath("/")))
        .Build();

    await container.StartAsync();

    var (stdout, stderr) = await container.GetLogsAsync();
    _output.WriteLine($"STDOUT:\n{stdout}");
    _output.WriteLine($"STDERR:\n{stderr}");
    _output.WriteLine($"Container ID: {container.Id}");
}
```

#### Common Issues

**Issue: Container startup timeout**

```csharp
var container = new ContainerBuilder("slow-starting-app:latest")
    .WithWaitStrategy(Wait.ForUnixContainer()
        .UntilHttpRequestIsSucceeded(r => r.ForPort(8080).ForPath("/"), o => o.WithTimeout(TimeSpan.FromMinutes(5))))
    .Build();
```

**Issue: Port already in use**
- Testcontainers auto-assigns random ports when using `.WithPortBinding(port, true)`
- Avoid fixed port bindings unless necessary
- Check for leaked containers: `docker ps -a`

**Issue: Image pull failures**

```bash
# Pull manually first to verify
docker pull postgres:16-alpine

# For private registries, login first
docker login registry.example.com
# Testcontainers will use credentials from Docker config
```

**Issue: Container not cleaning up**

```csharp
// Verify cleanup is enabled
var container = new ContainerBuilder("nginx:alpine3.23")
    .WithCleanUp(true)  // Ensure auto-cleanup is enabled (default: true)
    .Build();

// Check Ryuk is running
// docker ps | grep ryuk
// Windows PowerShell: docker ps | Select-String ryuk
// Windows CMD: docker ps | findstr ryuk
```

#### Environment Variables for Configuration

```csharp
// Custom Docker host
Environment.SetEnvironmentVariable("DOCKER_HOST", "tcp://localhost:2375");

// Disable Ryuk (not recommended)
Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED", "true");

// Custom Ryuk image
Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_CONTAINER_IMAGE", "testcontainers/ryuk:0.14.0");

// Hub image name prefix (for private registries)
Environment.SetEnvironmentVariable("TESTCONTAINERS_HUB_IMAGE_NAME_PREFIX", "my.registry.com/");
```

---

## Examples

### Example 1: PostgreSQL Integration Test

```csharp
// NuGet dependencies:
// - dotnet add package Npgsql
// - dotnet add package Testcontainers.PostgreSql
// - dotnet add package xunit.v3

using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

public sealed class UserRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("testdb")
        .WithUsername("testuser")
        .WithPassword("testpass")
        .Build();

    public async ValueTask InitializeAsync()
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

    public async ValueTask DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task CreateUser_ShouldInsertUser()
    {
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        await using var command = new NpgsqlCommand(
            "INSERT INTO users (name, email) VALUES (@name, @email) RETURNING id",
            connection);

        command.Parameters.AddWithValue("name", "Alice");
        command.Parameters.AddWithValue("email", "alice@example.com");

        var userId = await command.ExecuteScalarAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(userId);
    }

    [Fact]
    public async Task GetUser_ShouldReturnUser()
    {
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        await using var insertCmd = new NpgsqlCommand(
            "INSERT INTO users (name, email) VALUES (@name, @email)",
            connection);
        insertCmd.Parameters.AddWithValue("name", "Bob");
        insertCmd.Parameters.AddWithValue("email", "bob@example.com");
        await insertCmd.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);

        await using var selectCmd = new NpgsqlCommand(
            "SELECT name, email FROM users WHERE email = @email",
            connection);
        selectCmd.Parameters.AddWithValue("email", "bob@example.com");

        await using var reader = await selectCmd.ExecuteReaderAsync(TestContext.Current.CancellationToken);
        await reader.ReadAsync(TestContext.Current.CancellationToken);

        var name = reader.GetString(0);
        var email = reader.GetString(1);

        Assert.Equal("Bob", name);
        Assert.Equal("bob@example.com", email);
    }
}
```

### Example 2: Redis Cache Test

```csharp
// NuGet dependencies:
// - dotnet add package StackExchange.Redis
// - dotnet add package Testcontainers.Redis
// - dotnet add package xunit.v3

using StackExchange.Redis;
using Testcontainers.Redis;
using Xunit;

public sealed class RedisCacheTests : IAsyncLifetime
{
    private readonly RedisContainer _redis = new RedisBuilder("redis:7-alpine").Build();

    private IConnectionMultiplexer _connection;
    private IDatabase _db;

    public async ValueTask InitializeAsync()
    {
        await _redis.StartAsync();

        _connection = await ConnectionMultiplexer.ConnectAsync(_redis.GetConnectionString());
        _db = _connection.GetDatabase();
    }

    public async ValueTask DisposeAsync()
    {
        _connection.Dispose();
        await _redis.DisposeAsync();
    }

    [Fact]
    public async Task SetAndGet_ShouldStoreAndRetrieveValue()
    {
        await _db.StringSetAsync("key1", "value1");
        var value = await _db.StringGetAsync("key1");

        Assert.Equal("value1", value);
    }

    [Fact]
    public async Task SetWithExpiration_ShouldExpireKey()
    {
        await _db.StringSetAsync("key2", "value2", TimeSpan.FromSeconds(1));
        var valueBefore = await _db.StringGetAsync("key2");

        await Task.Delay(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);

        var valueAfter = await _db.StringGetAsync("key2");

        Assert.Equal("value2", valueBefore);
        Assert.True(valueAfter.IsNull);
    }
}
```

### Example 3: SQL Server with Entity Framework Core (SqlServer)

```csharp
// NuGet dependencies:
// - dotnet add package Microsoft.EntityFrameworkCore
// - dotnet add package Testcontainers.Mssql
// - dotnet add package xunit.v3

using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using Xunit;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
}

public sealed class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

public sealed class EntityFrameworkTests : IAsyncLifetime
{
    private readonly MsSqlContainer _mssql = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04").Build();

    private ApplicationDbContext _dbContext;

    public async ValueTask InitializeAsync()
    {
        await _mssql.StartAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_mssql.GetConnectionString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _mssql.DisposeAsync();
    }

    [Fact]
    public async Task AddUser_ShouldPersistToDatabase()
    {
        var user = new User
        {
            Name = "Alice",
            Email = "alice@example.com"
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var savedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == "alice@example.com", cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(savedUser);
        Assert.Equal("Alice", savedUser.Name);
    }
}
```

### Example 4: Kafka Producer/Consumer Test

```csharp
// NuGet dependencies:
// - dotnet add package Confluent.Kafka
// - dotnet add package Testcontainers.Kafka
// - dotnet add package xunit.v3

using Confluent.Kafka;
using Testcontainers.Kafka;
using Xunit;

public sealed class KafkaTests : IAsyncLifetime
{
    private readonly KafkaContainer _kafka = new KafkaBuilder("confluentinc/confluent-local:7.5.0").Build();

    public async ValueTask InitializeAsync()
    {
        await _kafka.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _kafka.DisposeAsync();
    }

    [Fact]
    public async Task ProduceAndConsume_ShouldTransferMessage()
    {
        const string topic = "test-topic";
        var bootstrapServers = _kafka.GetBootstrapAddress();

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };

        using var producer = new ProducerBuilder<string, string>(producerConfig).Build();

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = "test-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        consumer.Subscribe(topic);

        await producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = "key1",
            Value = "Hello, Kafka!"
        }, TestContext.Current.CancellationToken);

        var consumeResult = consumer.Consume(TimeSpan.FromSeconds(10));

        Assert.NotNull(consumeResult);
        Assert.Equal("Hello, Kafka!", consumeResult.Message.Value);
    }
}
```

### Example 5: ASP.NET Core WebApplicationFactory Integration

```csharp
// NuGet dependencies:
// - dotnet add package Microsoft.AspNetCore.Mvc.Testing
// - dotnet add package Testcontainers.PostgreSql
// - dotnet add package xunit.v3

using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;
using Xunit;

public sealed class ApiTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    public sealed class WebAppTests : WebApplicationFactory<Program>, IClassFixture<ApiTests>
    {
        private readonly string _connectionString;

        public WebAppTests(ApiTests fixture)
        {
            _connectionString = fixture._postgres.GetConnectionString();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Uses the .NET configuration system's connection-string support (ConnectionStrings:<Name>).
            builder.UseSetting("ConnectionStrings:Database", _connectionString);
        }

        [Fact]
        public async Task HealthCheck_ReturnsOk()
        {
            using var client = CreateClient();
            var response = await client.GetAsync("/health");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
```

---

### Example 6: Multi-Container Application Stack

```csharp
// NuGet dependencies:
// - dotnet add package Testcontainers.PostgreSql
// - dotnet add package Testcontainers.Redis
// - dotnet add package xunit.v3

using System.Net;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;

public sealed class FullStackTests : IAsyncLifetime
{
    private INetwork _network;
    private PostgreSqlContainer _postgres;
    private RedisContainer _redis;
    private IContainer _app;

    public async ValueTask InitializeAsync()
    {
        // Create network
        _network = new NetworkBuilder().Build();

        // Start PostgreSQL
        _postgres = new PostgreSqlBuilder("postgres:16-alpine")
            .WithNetwork(_network)
            .WithNetworkAliases("database")
            .Build();

        // Start Redis
        _redis = new RedisBuilder("redis:7-alpine")
            .WithNetwork(_network)
            .WithNetworkAliases("cache")
            .Build();

        _app = new ContainerBuilder("custom-image:latest")
            .WithNetwork(_network)
            .WithNetworkAliases("app")
            .WithEnvironment("DB_HOST", "database")
            .WithEnvironment("DB_PORT", "5432")
            .WithEnvironment("REDIS_HOST", "cache")
            .WithEnvironment("REDIS_PORT", "6379")
            .WithPortBinding(8080, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(8080).ForPath("/")))
            .Build();

        await _network.CreateAsync();
        await _postgres.StartAsync();
        await _redis.StartAsync();
        await _app.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _app.DisposeAsync();
        await _redis.DisposeAsync();
        await _postgres.DisposeAsync();
        await _network.DeleteAsync();
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnOk()
    {
        var endpoint = $"http://{_app.Hostname}:{_app.GetMappedPublicPort(8080)}";

        using var httpClient = new HttpClient();

        var response = await httpClient.GetAsync($"{endpoint}/health", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
```

---

## Best Practices

- **Always use pre-configured modules when available** - They provide sensible defaults and helper methods.
- **Use async lifecycle management** - Proper async initialization and cleanup (`IAsyncLifetime` in xUnit, `[OneTimeSetUp]`/`[OneTimeTearDown]` in NUnit, `[ClassInitialize]`/`[ClassCleanup]` in MSTest).
- **Always add wait strategies** - Ensures containers are ready before tests run; never use `Task.Delay()`/`Thread.Sleep()` as a readiness mechanism.
- **Use randomly assigned host ports** - Do not rely on fixed ports.
- **Copy configuration files into the container** - Do not rely on mounting files or directories.
- **Choose appropriate wait strategies** - Use HTTP for health endpoints, logs for startup messages, or commands for readiness.
- **Test against multiple configurations** - Use parameterized tests to validate versions/configurations (`Theory`/`InlineData` in xUnit, `TestCase` in NUnit, `DataRow` in MSTest).
- **Use custom networks** - For multi-container communication.
- **Keep containers ephemeral** - Do not rely on state between tests.
- **Share containers when appropriate** - Use fixtures or setup methods to share containers across tests for better performance.
- **Use module helper methods** - e.g., `GetConnectionString()`, `GetBootstrapAddress()`.
- **Debug with logs** - Use `GetLogsAsync()` when troubleshooting.
- **Use the builder pattern** - Fluent API for clear, maintainable configuration.

---

## Additional Resources

- **Official Documentation**: https://dotnet.testcontainers.org/
- **NuGet Packages**: https://www.nuget.org/packages?q=testcontainers
- **GitHub Repository**: https://github.com/testcontainers/testcontainers-dotnet
- **Examples**: https://github.com/testcontainers/testcontainers-dotnet/tree/develop/examples
    - https://github.com/testcontainers/testcontainers-dotnet/tree/develop/examples/Flyway
    - https://github.com/testcontainers/testcontainers-dotnet/tree/develop/examples/Respawn
- **Community Slack**: [testcontainers.slack.com](https://testcontainers.slack.com)
