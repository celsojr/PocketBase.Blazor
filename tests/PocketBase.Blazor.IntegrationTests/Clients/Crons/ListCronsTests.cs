namespace PocketBase.Blazor.IntegrationTests.Clients.Crons;

using Blazor.Responses.Cron;

[Collection("PocketBase.Blazor.Admin")]
public class ListCronsTests
{
    private readonly IPocketBase _pb;
    private readonly PocketBaseAdminFixture _fixture;

    public ListCronsTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
        _fixture = fixture;
    }

    [Fact]
    public async Task GetFullList_with_null_options_should_work()
    {
        // Act
        var result = await _pb.Crons.GetFullListAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetFullList_should_return_crons()
    {
        // Act
        var result = await _pb.Crons.GetFullListAsync(
            options: new CommonOptions {
                Fields = "expression:excerpt(3,true)",
                Query = new Dictionary<string, object?>
                {
                    ["perPage"] = "2"
                }
            });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeOfType<List<CronsResponse>>();
    }

    [Fact]
    public async Task GetFullList_with_cancellation_should_throw()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            _pb.Crons.GetFullListAsync(cancellationToken: cts.Token));
    }

    [Fact]
    public async Task List_crons_should_fail_when_not_admin()
    {
        // Arrange
        var client = new PocketBase(_fixture.Settings.BaseUrl);

        // Act
        var result = await client.Crons.GetFullListAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeNull();
        result.Errors[0].Message
            .Should().Contain("The request requires valid record authorization token.");
        result.Errors[0].Message.Should().Contain("401");
    }
}

