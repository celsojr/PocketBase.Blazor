namespace PocketBase.Blazor.IntegrationTests.Clients.Record;

using Blazor.Models;
using Blazor.Responses;
using Blazor.Responses.Auth;

[Trait("Category", "Integration")]
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
        var adminSession = default(AuthResponse);

        var id = $"{Guid.NewGuid():N}"[..6];
        var collectionName = $"no_otp_{id}";

        try
        {
            // Arrange - store the current user session for later
            var userSession = _pb.AuthStore.CurrentSession;
            userSession.Should().NotBeNull();

            // Admin creadentials is need for creating new collections
            await _pb.Admins
                .AuthWithPasswordAsync(
                    _fixture.Settings.AdminTesterEmail,
                    _fixture.Settings.AdminTesterPassword
                );

            // Save this admin session for cleaning up later
            adminSession = _pb.AuthStore.CurrentSession;
            adminSession.Should().NotBeNull();

            // Create a collection with OTP disabled (assuming default is disabled)
            var collection = await _pb.Collections.CreateAsync<CollectionModel>(new
            {
                name = collectionName,
                type = "auth",
                schema = new[]
                {
                    new { name = "email", type = "email", required = true }
                },
            });

            // Ensure the collection is created
            collection.IsSuccess.Should().BeTrue();
            collection.Value.Should().NotBeNull();

            // Create a test user in this collection
            var email = $"user_{id}@example.com";
            var testUserResult = await _pb.Collection(collectionName)
                .CreateAsync<RecordResponse>(new
                {
                    email,
                    password = "Np9wym_aAC4q8GWz",
                    passwordConfirm = "Np9wym_aAC4q8GWz"
                });

            // Ensure test user is created
            testUserResult.IsSuccess.Should().BeTrue();
            testUserResult.Value.Should().NotBeNull();

            // Switch back to the non-admin user
            _pb.AuthStore.Save(userSession);

            // Act - Should fail
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

    [Fact(Skip = "Requires SMTP server + configuration")]
    public async Task RequestOtpAsync_Succeeds_WhenEmailExists()
    {
        var adminSession = default(AuthResponse);
        var id = $"{Guid.NewGuid():N}"[..6];
        var collectionName = $"otp_enabled_{id}";
        try
        {
            // Arrange - Admin credentials needed for creating and updating collections
            await _pb.Admins
                .AuthWithPasswordAsync(
                    _fixture.Settings.AdminTesterEmail,
                    _fixture.Settings.AdminTesterPassword
                );

            // Save this admin session for cleaning up later
            adminSession = _pb.AuthStore.CurrentSession;
            adminSession.Should().NotBeNull();

            // Create a collection (no OTP option details for now)
            var collection = await _pb.Collections.CreateAsync<CollectionModel>(new
            {
                name = collectionName,
                type = "auth",
                schema = new[]
                {
                    new { name = "email", type = "email", required = true, unique = true }
                },
            });

            // Ensure the collection is created
            collection.IsSuccess.Should().BeTrue();
            collection.Value.Should().NotBeNull();

            // Update the collection to enable OTP (just an update example)
            var updateResult = await _pb.Collections.UpdateAsync<CollectionModel>(
                collectionName,
                options: new CommonOptions()
                {
                    Body = new
                    {
                        // Explicitly enable OTP with default values
                        otp = new
                        {
                            enabled = true
                        }
                    }
                }
            );

            // Ensure a successful response
            updateResult.IsSuccess.Should().BeTrue();

            // Create a test user in this collection
            var email = $"user_{id}@example.com";
            var record = default(Result<RecordResponse>);
            record = await _pb.Collection(collectionName)
                .CreateAsync<RecordResponse>(new
                {
                    email,
                    password = "password123",
                    passwordConfirm = "password123"
                });

            // Ensure test user is created
            record.IsSuccess.Should().BeTrue();
            record.Value.Should().NotBeNull();

            // Act - Request OTP for the user
            var result = await _pb.Collection(collectionName)
                .RequestOtpAsync(email);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.OtpId.Should().NotBeNullOrEmpty();
        }
        finally
        {
            // Clean up
            if (adminSession != null)
            {
                _pb.AuthStore.Save(adminSession);
                await _pb.Collections.DeleteAsync(collectionName);
            }
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
