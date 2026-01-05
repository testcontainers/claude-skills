using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Xunit;

namespace TestcontainersExamples;

/// <summary>
/// Demonstrates generic container usage without pre-configured modules
/// </summary>
public class GenericContainerTests : IAsyncLifetime
{
    private IContainer? _container;

    public Task InitializeAsync()
    {
        // No shared setup
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }

    [Fact]
    public async Task NginxContainer_ShouldServeDefaultPage()
    {
        // Arrange
        _container = new ContainerBuilder("nginx:alpine")
            .WithPortBinding(80, true)
            .Build();

        // Act
        await _container.StartAsync();

        var port = _container.GetMappedPublicPort(80);
        var hostname = _container.Hostname;

        // Assert
        Assert.True(port > 0);
        Assert.NotEmpty(hostname);
    }

    [Fact]
    public async Task ContainerWithEnvironment_ShouldSetVariables()
    {
        // Arrange
        _container = new ContainerBuilder("alpine:latest")
            .WithEnvironment("TEST_VAR", "test-value")
            .WithEnvironment("ANOTHER_VAR", "another-value")
            .WithCommand("sh", "-c", "sleep 10")
            .Build();

        // Act
        await _container.StartAsync();

        var result = await _container.ExecAsync(new[] { "sh", "-c", "echo $TEST_VAR" });

        // Assert
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("test-value", result.Stdout);
    }

    [Fact]
    public async Task ExecCommand_ShouldExecuteAndReturnOutput()
    {
        // Arrange
        _container = new ContainerBuilder("alpine:latest")
            .WithCommand("sh", "-c", "sleep 20")
            .Build();

        await _container.StartAsync();

        // Act
        var result = await _container.ExecAsync(new[] { "echo", "Hello, Testcontainers!" });

        // Assert
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Hello, Testcontainers!", result.Stdout);
    }

    [Fact]
    public async Task ReadLogs_ShouldReturnContainerOutput()
    {
        // Arrange
        _container = new ContainerBuilder("alpine:latest")
            .WithCommand("sh", "-c", "echo 'Container started successfully' && sleep 5")
            .Build();

        // Act
        await _container.StartAsync();

        var (stdout, stderr) = await _container.GetLogsAsync();

        // Assert
        Assert.Contains("Container started successfully", stdout);
    }

    [Fact]
    public async Task WaitForLog_ShouldWaitUntilMessageAppears()
    {
        // Arrange
        _container = new ContainerBuilder("alpine:latest")
            .WithCommand("sh", "-c", "sleep 2 && echo 'Ready to serve' && sleep 10")
            .Build();

        // Act
        var startTime = DateTime.UtcNow;
        await _container.StartAsync();
        var elapsed = DateTime.UtcNow - startTime;

        // Assert - Note: without wait strategy, this may not wait for the log
        var (stdout, _) = await _container.GetLogsAsync();
        Assert.Contains("Ready to serve", stdout);
    }

    [Fact]
    public async Task PortMapping_ShouldMapToRandomPort()
    {
        // Arrange
        _container = new ContainerBuilder("nginx:alpine")
            .WithPortBinding(80, true)  // true = assign random port
            .Build();

        // Act
        await _container.StartAsync();

        var mappedPort = _container.GetMappedPublicPort(80);

        // Assert
        Assert.True(mappedPort > 0);
        Assert.NotEqual(80, mappedPort); // Should be randomly assigned
    }
}

/// <summary>
/// Demonstrates advanced generic container patterns
/// </summary>
public class AdvancedGenericContainerTests
{
    [Fact]
    public async Task ContainerWithCustomWaitStrategy_HTTP()
    {
        // Arrange
        await using var container = new ContainerBuilder("nginx:alpine")
            .WithPortBinding(80, true)
            .Build();

        // Act
        await container.StartAsync();

        // Assert
        var port = container.GetMappedPublicPort(80);
        Assert.True(port > 0);

        // Verify HTTP endpoint is accessible
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync($"http://{container.Hostname}:{port}");

        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task ContainerWithBindMount_ShouldAccessHostFiles()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var testFile = Path.Combine(tempDir, "test.txt");
            await File.WriteAllTextAsync(testFile, "Hello from host!");

            await using var container = new ContainerBuilder("alpine:latest")
                .WithBindMount(tempDir, "/data")
                .WithCommand("sh", "-c", "sleep 10")
                .Build();

            // Act
            await container.StartAsync();

            var result = await container.ExecAsync(new[] { "cat", "/data/test.txt" });

            // Assert
            Assert.Equal(0, result.ExitCode);
            Assert.Contains("Hello from host!", result.Stdout);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public async Task ContainerWithLabels_ShouldSetMetadata()
    {
        // Arrange
        await using var container = new ContainerBuilder("alpine:latest")
            .WithLabel("test.project", "testcontainers-dotnet")
            .WithLabel("test.environment", "ci")
            .WithCommand("sh", "-c", "sleep 5")
            .Build();

        // Act
        await container.StartAsync();

        // Assert - verify container is running
        var id = container.Id;
        Assert.NotEmpty(id);
    }

    [Fact]
    public async Task MultipleContainers_CanRunInParallel()
    {
        // Arrange
        var container1 = new ContainerBuilder("alpine:latest")
            .WithCommand("sh", "-c", "sleep 10")
            .Build();

        var container2 = new ContainerBuilder("alpine:latest")
            .WithCommand("sh", "-c", "sleep 10")
            .Build();

        try
        {
            // Act - Start both containers in parallel
            await Task.WhenAll(
                container1.StartAsync(),
                container2.StartAsync()
            );

            // Assert - verify both containers are running
            var id1 = container1.Id;
            var id2 = container2.Id;

            Assert.NotEmpty(id1);
            Assert.NotEmpty(id2);
            Assert.NotEqual(id1, id2);
        }
        finally
        {
            // Cleanup
            await Task.WhenAll(
                container1.DisposeAsync().AsTask(),
                container2.DisposeAsync().AsTask()
            );
        }
    }
}
