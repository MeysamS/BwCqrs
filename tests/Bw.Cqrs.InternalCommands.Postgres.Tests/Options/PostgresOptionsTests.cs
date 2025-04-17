using Bw.Cqrs.InternalCommands.Postgres.Models;
using Xunit;

namespace Bw.Cqrs.Commands.Postgres.Tests.Options;

public class PostgresOptionsTests
{
    [Fact]
    public void Validate_WithValidOptions_ShouldNotThrow()
    {
        // Arrange
        var options = new PostgresOptions
        {
            ConnectionString = "Host=localhost;Database=test;Username=test;Password=test",
            CommandTimeout = TimeSpan.FromSeconds(30)
        };

        // Act & Assert
        var exception = Record.Exception(() => options.Validate());
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_WithInvalidConnectionString_ShouldThrow()
    {
        // Arrange
        var options = new PostgresOptions
        {
            ConnectionString = string.Empty,
            CommandTimeout = TimeSpan.FromSeconds(30)
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => options.Validate());
    }

    [Fact]
    public void Validate_WithInvalidCommandTimeout_ShouldThrow()
    {
        // Arrange
        var options = new PostgresOptions
        {
            ConnectionString = "Host=localhost;Database=test;Username=test;Password=test",
            CommandTimeout = TimeSpan.Zero
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => options.Validate());
    }
} 