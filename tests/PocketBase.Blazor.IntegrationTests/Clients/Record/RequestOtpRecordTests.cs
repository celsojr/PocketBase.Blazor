namespace PocketBase.Blazor.IntegrationTests.Clients.Record;

using Blazor.Responses;

[Collection("PocketBase.Blazor.User")]
public class RequestOtpRecordTests
{
    private readonly IPocketBase _pb;
    private readonly PocketBaseUserFixture _fixture;

    public RequestOtpRecordTests(PocketBaseUserFixture fixture)
    {
        _fixture = fixture;
        _pb = fixture.Client;
    }

    [Fact]
    public async Task RequestOtpAsync_Succeeds_WhenEmailExists()
    {
        var record = default(Result<RecordResponse>);
        try
        {
            // Arrange
            var id = $"{Guid.NewGuid():N}"[..6];
            var email = $"user_{id}@example.com";

            record = await _pb.Collection("users")
                .CreateAsync<RecordResponse>(new
                {
                    email,
                    password = "password123",
                    passwordConfirm = "password123"
                });

            // Act
            var result = await _pb.Collection("users")
                .RequestOtpAsync(email);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.OtpId.Should().NotBeNullOrEmpty();
        }
        finally
        {
            await _pb.Admins
                .AuthWithPasswordAsync(
                    _fixture.Settings.AdminTesterEmail,
                    _fixture.Settings.AdminTesterPassword
                );

            // Clean up - Doesn't work for non-admins
            await _pb.Collection("users").DeleteAsync(record?.Value.Id);
        }
    }

    [Fact]
    public async Task RequestOtpAsync_Throws_WhenEmailEmpty()
    {
        await FluentActions
            .Awaiting(() => _pb.Collection("users").RequestOtpAsync(""))
            .Should().ThrowAsync<ArgumentException>();
    }
}
