namespace PocketBase.Blazor.IntegrationTests.Clients.Crons;

[Trait("Category", "Integration")]
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
        Result<IEnumerable<Responses.Cron.CronsResponse>> cronList = await _pb.Crons.GetFullListAsync();
        Responses.Cron.CronsResponse? existingCron = cronList.Value.FirstOrDefault();
 
        existingCron.Should().NotBeNull();
        existingCron.Id.Should().NotBeNullOrWhiteSpace();

        // Act
        Result result = await _pb.Crons.RunAsync(existingCron.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RunAsync_with_invalid_id_should_fail()
    {
        // Act
        Result result = await _pb.Crons.RunAsync("non_existent_id");

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
        Result result = await _pb.Crons.RunAsync(logsCleanupId);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RunAsync_with_null_id_should_throw()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _pb.Crons.RunAsync(null!));
    }

    [Fact]
    public async Task RunAsync_with_empty_id_should_throw()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _pb.Crons.RunAsync(""));
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
        await using PocketBase client = new PocketBase(_fixture.Settings.BaseUrl);
        const string logsCleanupId = "__pbLogsCleanup__";

        // Act
        Result result = await client.Crons.RunAsync(logsCleanupId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeNull();

        result.Errors[0].Message
            .Should().Contain("The request requires valid record authorization token.");
        result.Errors[0].Message.Should().Contain("401");
    }
}
