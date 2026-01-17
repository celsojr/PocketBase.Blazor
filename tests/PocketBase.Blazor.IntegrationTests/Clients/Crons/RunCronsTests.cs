namespace PocketBase.Blazor.IntegrationTests.Clients.Crons;

[Collection("PocketBase.Blazor.Admin")]
public class RunCronsTests
{
    private readonly IPocketBase _pb;
    private readonly PocketBaseAdminFixture _fixture;

    public RunCronsTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
        _fixture = fixture;
    }

    [Fact]
    public async Task RunAsync_with_valid_id_should_succeed()
    {
        // Arrange
        var cronList = await _pb.Crons.GetFullListAsync();
        var existingCron = cronList.Value.FirstOrDefault();
    
        existingCron.Should().NotBeNull();
        existingCron.Id.Should().NotBeNullOrWhiteSpace();

        // Act
        var result = await _pb.Crons.RunAsync(existingCron.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RunAsync_with_invalid_id_should_fail()
    {
        // Act
        var result = await _pb.Crons.RunAsync("non_existent_id");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RunAsync_with_logs_cleanup_cron_should_succeed()
    {
        // Arrange - Get the built-in logs cleanup cron
        const string logsCleanupId = "__pbLogsCleanup__";

        // Act
        var result = await _pb.Crons.RunAsync(logsCleanupId);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void RunAsync_with_null_id_should_throw()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _pb.Crons.RunAsync(null!));
    }

    [Fact]
    public void RunAsync_with_empty_id_should_throw()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _pb.Crons.RunAsync(""));
    }

    [Fact]
    public async Task RunAsync_with_whitespace_id_should_throw()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _pb.Crons.RunAsync("   "));
    }

    [Fact]
    public async Task Running_crons_should_fail_when_not_admin()
    {
        // Arrange - Get the built-in logs cleanup cron
        var client = new PocketBase(_fixture.Settings.BaseUrl);
        const string logsCleanupId = "__pbLogsCleanup__";

        // Act
        var result = await client.Crons.RunAsync(logsCleanupId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeNull();

        result.Errors[0].Message
            .Should().Contain("The request requires valid record authorization token.");
        result.Errors[0].Message.Should().Contain("401");
    }
}

