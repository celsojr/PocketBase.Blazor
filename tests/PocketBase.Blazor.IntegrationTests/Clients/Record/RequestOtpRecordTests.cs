namespace PocketBase.Blazor.IntegrationTests.Clients.Record;

using Blazor.Models;
using Blazor.Responses;
using Blazor.Responses.Auth;

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
    public async Task RequestOtpAsync_Fails_WhenCollectionNotConfiguredForOtp()
    {
        // Arrange
        var collection = default(Result<CollectionModel>);
        var adminSession = default(AuthResponse);

        var id = $"{Guid.NewGuid():N}"[..6];
        var collectionName = $"no_otp_{id}";

        try
        {
            // Create a custom collection without OTP enabled
            var userSession = _pb.AuthStore.CurrentSession;
            userSession.Should().NotBeNull();

            // Admin creadentials is need for creating new collections
            await _pb.Admins
                .AuthWithPasswordAsync(
                    _fixture.Settings.AdminTesterEmail,
                    _fixture.Settings.AdminTesterPassword
                );

            // Save this for later clean up
            adminSession = _pb.AuthStore.CurrentSession;
            adminSession.Should().NotBeNull();

            // Create a collection with OTP disabled (assuming default is disabled)
            collection = await _pb.Collections.CreateAsync<CollectionModel>(new
            {
                name = collectionName,
                type = "auth",
                schema = new[]
                {
                    new { name = "email", type = "email", required = true }
                },
            },
            new CommonOptions()
            {
                Body = new
                {
                    // Explicitly disable OTP if your PocketBase version supports it
                    otp = new
                    {
                        enabled = false
                    }
                }
            });

            // Ensure created
            collection.IsSuccess.Should().BeTrue();
            collection.Value.Should().NotBeNull();

            // Create a test user in this collection
            var email = $"user_{id}@example.com";
            await _pb.Collection(collectionName)
                .CreateAsync<RecordResponse>(new
                {
                    email
                });

            // Switch back as a user
            _pb.AuthStore.Save(userSession);

            // Act
            var result = await _pb.Collection(collectionName)
                .RequestOtpAsync(email);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().NotBeNull();
            result.Errors[0].Message.Should().Contain("403");
            result.Errors[0].Message.Should().Contain("not configured to allow OTP authentication");
        }
        finally
        {
            // Clean up
            if (adminSession != null)
            {
                _pb.AuthStore.Save(adminSession);
                await _pb.Collections
                    .DeleteAsync(collectionName);
            }
        }
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
