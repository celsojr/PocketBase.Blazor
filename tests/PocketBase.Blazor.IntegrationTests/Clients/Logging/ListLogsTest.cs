namespace PocketBase.Blazor.IntegrationTests.Clients.Logging;

using Blazor.Enums;
using Blazor.Models;
using Blazor.Responses.Logging;

[Collection("PocketBase.Blazor.Admin")]
public class ListLogsTests
{
    private readonly IPocketBase _pb;
    private readonly PocketBaseAdminFixture _fixture;

    public ListLogsTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
        _fixture = fixture;
    }

    [Fact]
    public async Task GetListAsync_ReturnsLogs_WhenDefaultParams()
    {
        // Act
        var result = await _pb.Log.GetListAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().NotBeNull();
        result.Value.Should().BeOfType<ListResult<LogResponse>>();
    }

    [Fact(Skip = "May fail if there not enough logs")]
    public async Task GetListAsync_ReturnsPaginatedLogs_WhenPageSpecified()
    {
        // Act
        var result = await _pb.Log.GetListAsync(page: 2, perPage: 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Page.Should().Be(2);
        result.Value.PerPage.Should().Be(10);
    }

    [Fact(Skip = "May fail if there not enough logs")]
    public async Task GetListAsync_ReturnsFilteredLogs_WhenOptionsProvided()
    {
        // Arrange
        var options = new ListOptions
        {
            Filter = $"level={(int)LogLevel.Error}",
            Sort = "-created",
            Page = 1,
            PerPage = 500, // max value from Pocketbase
            SkipTotal = true // Performance optimization
        };

        // Act
        var result = await _pb.Log.GetListAsync(options: options);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().OnlyContain(log => log.Level == LogLevel.Error);
    }
}
