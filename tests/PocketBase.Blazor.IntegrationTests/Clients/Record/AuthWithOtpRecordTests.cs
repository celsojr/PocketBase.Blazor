namespace PocketBase.Blazor.IntegrationTests.Clients.Record;

using Blazor.Models;
using Blazor.Responses;
using Blazor.Responses.Auth;
using Helpers.MailHog;

[Trait("Category", "Integration")]
[Trait("Requires", "SMTP")]
[Collection("PocketBase.Blazor.User")]
public class AuthWithOtpRecordTests
{
    private readonly IPocketBase _pb;
    private readonly MailHogService _mailHogService;
    private readonly PocketBaseUserFixture _fixture;

    public AuthWithOtpRecordTests(PocketBaseUserFixture fixture)
    {
        _fixture = fixture;
        _pb = fixture.Client;

        var options = new MailHogOptions
        { 
            BaseUrl = "http://localhost:8027"
        };
        _mailHogService = new MailHogService(new HttpClient(), options);
    }

    [Fact]
    public async Task AuthWithOtpAsync_Succeeds_WithValidOtp()
    {
        var adminSession = default(AuthResponse);
        var id = $"{Guid.NewGuid():N}"[..6];
        var collectionName = $"otp_auth_{id}";

        try
        {
            // Arrange - Configure SMTP with MailHog
            await _pb.Admins.AuthWithPasswordAsync(
                _fixture.Settings.AdminTesterEmail,
                _fixture.Settings.AdminTesterPassword
            );

            // Store the admin session for cleaning up later
            adminSession = _pb.AuthStore.CurrentSession;

            // Configure SMTP to use MailHog
            var smtpResult = await _pb.Settings.UpdateAsync(new
            {
                smtp = new
                {
                    enabled = true,
                    host = "localhost",
                    port = 1027, // MailHog SMTP port
                    tls = false
                }
            });
            smtpResult.IsSuccess.Should().BeTrue();

            // Create collection with OTP enabled
            var collection = await _pb.Collections.CreateAsync<CollectionModel>(new
            {
                name = collectionName,
                type = "auth",
                schema = new[]
                {
                    new { name = "email", type = "email", required = true, unique = true }
                },
            },
            new CommonOptions()
            {
                Body = new Dictionary<string, object?>
                {
                    // Use the Pocketbase default values
                    ["otp"] = new Dictionary<string, object?>
                    {
                        ["enabled"] = true
                    }
                }
            });
            collection.IsSuccess.Should().BeTrue();

            // Create a test user for this collection
            var email = $"user_{id}@example.com";
            var userResult = await _pb.Collection(collectionName)
                .CreateAsync<RecordResponse>(new
                {
                    email,
                    password = "password123",
                    passwordConfirm = "password123",
                    verified = true // Ensure email is verified for OTP
                });
            userResult.IsSuccess.Should().BeTrue();

            // Request OTP first
            var otpRequest = await _pb.Collection(collectionName)
                .RequestOtpAsync(email);
            otpRequest.IsSuccess.Should().BeTrue();
            otpRequest.Value.OtpId.Should().NotBeNullOrEmpty();

            // Get the OTP code from MailHog
            // Note: Wait a moment for the email to be delivered
            await Task.Delay(1000);

            var otpCode = await _mailHogService.GetLatestOtpCodeAsync(email);
            otpCode.Should().NotBeNullOrEmpty();

            // Act - Authenticate with OTP
            // Note: If the first authentication attempt fails, the OTP may become invalid
            var authResult = await _pb.Collection(collectionName)
                .AuthWithOtpAsync(otpRequest.Value.OtpId, otpCode);

            // Assert
            authResult.IsSuccess.Should().BeTrue();
            authResult.Value.Should().NotBeNull();
            authResult.Value.Record.Should().NotBeNull();
            authResult.Value.Record.Email.Should().Be(email);
            authResult.Value.Token.Should().NotBeNullOrEmpty();
        }
        finally
        {
            // Clean up
            if (adminSession != null)
            {
                _pb.AuthStore.Save(adminSession);
                await _pb.Collections.DeleteAsync(collectionName);
            }

            // Attempt to clear MailHog messages, but don't fail if it doesn't work
            try { await _mailHogService.ClearAllMessagesAsync(); } catch(Exception) { };
        }
    }

    [Fact]
    public async Task AuthWithOtpAsync_Fails_WithInvalidOtpCode()
    {
        var adminSession = default(AuthResponse);
        var id = $"{Guid.NewGuid():N}"[..6];
        var collectionName = $"otp_auth_{id}";

        try
        {
            // Arrange - Configure SMTP with MailHog
            await _pb.Admins.AuthWithPasswordAsync(
                _fixture.Settings.AdminTesterEmail,
                _fixture.Settings.AdminTesterPassword
            );

            // Store the admin session for cleaning up later
            adminSession = _pb.AuthStore.CurrentSession;

            // Configure SMTP
            var smtpResult = await _pb.Settings.UpdateAsync(new
            {
                smtp = new
                {
                    enabled = true,
                    host = "localhost",
                    port = 1027,
                    tls = false
                }
            });
            smtpResult.IsSuccess.Should().BeTrue();

            // Create collection with OTP enabled
            var collection = await _pb.Collections.CreateAsync<CollectionModel>(new
            {
                name = collectionName,
                type = "auth",
                schema = new[]
                {
                    new { name = "email", type = "email", required = true, unique = true }
                },
            },
            new CommonOptions()
            {
                Body = new Dictionary<string, object?>
                {
                    // Use the Pocketbase default values
                    ["otp"] = new Dictionary<string, object?>
                    {
                        ["enabled"] = true
                    }
                }
            });
            collection.IsSuccess.Should().BeTrue();

            // Create test user for this collection
            var email = $"user_{id}@example.com";
            var userResult = await _pb.Collection(collectionName)
                .CreateAsync<RecordResponse>(new
                {
                    email,
                    password = "password123",
                    passwordConfirm = "password123",
                    verified = true
                });
            userResult.IsSuccess.Should().BeTrue();

            // Request OTP - SMTP server is not needed at this point
            var otpRequest = await _pb.Collection(collectionName)
                .RequestOtpAsync(email);
            otpRequest.IsSuccess.Should().BeTrue();

            // Act - Try with invalid OTP code
            var authResult = await _pb.Collection(collectionName)
                .AuthWithOtpAsync(otpRequest.Value.OtpId, "invalid_code_123");

            // Assert
            authResult.IsSuccess.Should().BeFalse();
            authResult.Errors.Should().NotBeNull();
            authResult.Errors[0].Message.Should().Contain("400");
            authResult.Errors[0].Message.Should().Contain("Invalid or expired OTP");
        }
        finally
        {
            // Clean up
            if (adminSession != null)
            {
                _pb.AuthStore.Save(adminSession);
                await _pb.Collections.DeleteAsync(collectionName);
            }

            // Attempt to clear MailHog messages, but don't fail if it doesn't work
            try { await _mailHogService.ClearAllMessagesAsync(); } catch(Exception) { };
        }
    }

    [Fact]
    public async Task AuthWithOtpAsync_Fails_WithExpiredOtp()
    {
        var adminSession = default(AuthResponse);
        var id = $"{Guid.NewGuid():N}"[..6];
        var collectionName = $"otp_auth_{id}";

        try
        {
            // Arrange
            await _pb.Admins.AuthWithPasswordAsync(
                _fixture.Settings.AdminTesterEmail,
                _fixture.Settings.AdminTesterPassword
            );

            // Store the admin session for cleaning up later
            adminSession = _pb.AuthStore.CurrentSession;

            // Configure SMTP
            var smtpResult = await _pb.Settings.UpdateAsync(new
            {
                smtp = new
                {
                    enabled = true,
                    host = "localhost",
                    port = 1027,
                    tls = false
                }
            });
            smtpResult.IsSuccess.Should().BeTrue();

            // Create collection with OTP enabled and short expiration
            var collection = await _pb.Collections.CreateAsync<CollectionModel>(new
            {
                name = collectionName,
                type = "auth",
                schema = new[]
                {
                    new { name = "email", type = "email", required = true, unique = true }
                },
            },
            new CommonOptions()
            {
                Body = new Dictionary<string, object?>
                {
                    // Use the Pocketbase default values
                    ["otp"] = new Dictionary<string, object?>
                    {
                        ["enabled"] = true,
                        ["duration"] = 10 // Must be no less than 10
                    }
                }
            });
            collection.IsSuccess.Should().BeTrue();

            // Create test user
            var email = $"user_{id}@example.com";
            var userResult = await _pb.Collection(collectionName)
                .CreateAsync<RecordResponse>(new
                {
                    email,
                    password = "password123",
                    passwordConfirm = "password123",
                    verified = true // Ensure email is verified for OTP
                });
            userResult.IsSuccess.Should().BeTrue();

            // Request OTP
            var otpRequest = await _pb.Collection(collectionName)
                .RequestOtpAsync(email);
            otpRequest.IsSuccess.Should().BeTrue();

            // Wait for OTP to expire
            await Task.Delay(TimeSpan.FromSeconds(20));

            // Get the OTP code from MailHog (though it should be expired)
            var otpCode = await _mailHogService.GetLatestOtpCodeAsync(email);
            otpCode.Should().NotBeNull();

            // Act - Try with expired OTP
            var authResult = await _pb.Collection(collectionName)
                .AuthWithOtpAsync(otpRequest.Value.OtpId, otpCode);

            // Assert
            authResult.IsSuccess.Should().BeFalse();
            authResult.Errors.Should().NotBeNull();
            authResult.Errors[0].Message.Should().Contain("400");
            authResult.Errors[0].Message.Should().Contain("Invalid or expired OTP");
        }
        finally
        {
            // Clean up
            if (adminSession != null)
            {
                _pb.AuthStore.Save(adminSession);
                await _pb.Collections.DeleteAsync(collectionName);
            }

            // Attempt to clear MailHog messages, but don't fail if it doesn't work
            try { await _mailHogService.ClearAllMessagesAsync(); } catch(Exception) { };
        }
    }
}
