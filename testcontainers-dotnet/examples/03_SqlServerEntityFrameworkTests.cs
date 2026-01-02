using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using Xunit;

namespace TestcontainersExamples;

/// <summary>
/// Sample Entity Framework DbContext
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
}

/// <summary>
/// Sample entity
/// </summary>
public class User
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Demonstrates SQL Server container with Entity Framework Core
/// </summary>
public class SqlServerEntityFrameworkTests : IAsyncLifetime
{
    private readonly MsSqlContainer _mssql = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    private ApplicationDbContext? _dbContext;

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
        if (_dbContext != null)
        {
            await _dbContext.DisposeAsync();
        }
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
        _dbContext!.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == "alice@example.com");

        Assert.NotNull(savedUser);
        Assert.Equal("Alice", savedUser.Name);
        Assert.True(savedUser.Id > 0);
    }

    [Fact]
    public async Task UpdateUser_ShouldModifyExistingUser()
    {
        // Arrange
        var user = new User
        {
            Name = "Bob",
            Email = "bob@example.com"
        };

        _dbContext!.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        user.Name = "Robert";
        await _dbContext.SaveChangesAsync();

        // Assert
        var updatedUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == "bob@example.com");

        Assert.NotNull(updatedUser);
        Assert.Equal("Robert", updatedUser.Name);
    }

    [Fact]
    public async Task DeleteUser_ShouldRemoveFromDatabase()
    {
        // Arrange
        var user = new User
        {
            Name = "Charlie",
            Email = "charlie@example.com"
        };

        _dbContext!.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();

        // Assert
        var deletedUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == "charlie@example.com");

        Assert.Null(deletedUser);
    }

    [Fact]
    public async Task QueryUsers_ShouldReturnFilteredResults()
    {
        // Arrange
        var users = new[]
        {
            new User { Name = "Alice", Email = "alice@example.com" },
            new User { Name = "Bob", Email = "bob@example.com" },
            new User { Name = "Charlie", Email = "charlie@example.com" }
        };

        _dbContext!.Users.AddRange(users);
        await _dbContext.SaveChangesAsync();

        // Act
        var filteredUsers = await _dbContext.Users
            .Where(u => u.Name.StartsWith("A") || u.Name.StartsWith("C"))
            .OrderBy(u => u.Name)
            .ToListAsync();

        // Assert
        Assert.Equal(2, filteredUsers.Count);
        Assert.Equal("Alice", filteredUsers[0].Name);
        Assert.Equal("Charlie", filteredUsers[1].Name);
    }
}
