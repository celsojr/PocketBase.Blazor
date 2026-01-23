namespace PocketBase.Blazor.IntegrationTests.Clients.Logging;

using Blazor.Responses;

[Collection("PocketBase.Blazor.Admin")]
public class GetLogStatsTests
{
    private readonly IPocketBase _pb;

    public GetLogStatsTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
    }

    [Fact]
    public async Task GetStatsAsync_ReturnsStats_WhenDefaultOptions()
    {
        // Act
        var result = await _pb.Log.GetStatsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeOfType<List<HourlyStatsResponse>>();
    }

    [Fact]
    public async Task GetStatsAsync_ReturnsFilteredStats_WhenFilterProvided()
    {
        // Arrange
        var options = new LogStatsOptions
        {
            Filter = "level > 0" // Error logs only
        };

        // Act
        var result = await _pb.Log.GetStatsAsync(options);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetStatsAsync_ReturnsStatsWithDateRange_WhenDatesProvided()
    {
        // Arrange
        var yesterday = DateTime.UtcNow.AddDays(-1);
        var today = DateTime.UtcNow;
        
        var options = new LogStatsOptions
        {
            Filter = $"created >= '{yesterday:yyyy-MM-dd}' && created <= '{today:yyyy-MM-dd}'"
        };

        // Act
        var result = await _pb.Log.GetStatsAsync(options);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetStatsAsync_ReturnsStatsByType_WhenTypeFiltered()
    {
        // Arrange
        var options = new LogStatsOptions
        {
            Filter = "type = 'request'"
        };

        // Act
        var result = await _pb.Log.GetStatsAsync(options);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }
}

