namespace PocketBase.Blazor.IntegrationTests.Clients.Record;

using Blazor.Extensions;
using Blazor.Models;
using Blazor.Responses;
using Blazor.Responses.Auth;
using Helpers.MailHog;

[Trait("Category", "Integration")]
[Trait("Requires", "SMTP")]
[Collection("PocketBase.Blazor.User")]
public class AuthWithMfaRecordTests
{
    private readonly IPocketBase _pb;
    private readonly MailHogService _mailHogService;
    private readonly PocketBaseUserFixture _fixture;

    public AuthWithMfaRecordTests(PocketBaseUserFixture fixture)
    {
        _fixture = fixture;
        _pb = fixture.Client;

        _mailHogService = new MailHogService(new HttpClient(), new MailHogOptions
        {
            BaseUrl = "http://localhost:8027"
        });
    }

    [Fact]
    public async Task AuthWithPasswordAsync_ReturnsMfaId_WhenSecondFactorIsRequired()
    {
        (string collectionName, string email) = await SetupMfaCollectionAsync();
        AuthResponse? adminSession = _pb.AuthStore.CurrentSession;

        try
        {
            // Act - first factor only
            Result<AuthResponse> result = await _pb.Collection(collectionName)
                .AuthWithPasswordAsync(email, "password123");

            // Assert - PocketBase answers 401 and hands back an MFA session id
            result.IsSuccess.Should().BeFalse();
            result.GetStatusCode().Should().Be(401);
            result.GetMfaId().Should().NotBeNullOrEmpty();
        }
        finally
        {
            await CleanupAsync(collectionName, adminSession);
        }
    }

    [Fact]
    public async Task AuthWithOtpAsync_Succeeds_WhenMfaIdIsPassed()
    {
        (string collectionName, string email) = await SetupMfaCollectionAsync();
        AuthResponse? adminSession = _pb.AuthStore.CurrentSession;

        try
        {
            // Act - 1st factor (password) to obtain the MFA session
            Result<AuthResponse> first = await _pb.Collection(collectionName)
                .AuthWithPasswordAsync(email, "password123");

            first.IsSuccess.Should().BeFalse();
            string? mfaId = first.GetMfaId();
            mfaId.Should().NotBeNullOrEmpty();

            // Request the 2nd factor (OTP) and pull the code from MailHog
            Result<RequestOtpResponse> otpRequest = await _pb.Collection(collectionName)
                .RequestOtpAsync(email);
            otpRequest.IsSuccess.Should().BeTrue();
            otpRequest.Value.OtpId.Should().NotBeNullOrEmpty();

            await Task.Delay(1000);
            string? otpCode = await _mailHogService.GetLatestOtpCodeAsync(email);
            otpCode.Should().NotBeNullOrEmpty();

            // Act - 2nd factor with the MFA session id completes the flow
            Result<AuthResponse> result = await _pb.Collection(collectionName)
                .AuthWithOtpAsync(otpRequest.Value.OtpId, otpCode, mfaId: mfaId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Token.Should().NotBeNullOrEmpty();
            result.Value.Record.Should().NotBeNull();
            result.Value.Record.Email.Should().Be(email);
        }
        finally
        {
            await CleanupAsync(collectionName, adminSession);
        }
    }

    [Fact]
    public async Task AuthWithOtpAsync_Fails_WhenMfaIdIsMissing()
    {
        (string collectionName, string email) = await SetupMfaCollectionAsync();
        AuthResponse? adminSession = _pb.AuthStore.CurrentSession;

        try
        {
            // Arrange - open an MFA session with the 1st factor
            Result<AuthResponse> first = await _pb.Collection(collectionName)
                .AuthWithPasswordAsync(email, "password123");
            first.IsSuccess.Should().BeFalse();
            first.GetMfaId().Should().NotBeNullOrEmpty();

            Result<RequestOtpResponse> otpRequest = await _pb.Collection(collectionName)
                .RequestOtpAsync(email);
            otpRequest.IsSuccess.Should().BeTrue();

            await Task.Delay(1000);
            string? otpCode = await _mailHogService.GetLatestOtpCodeAsync(email);
            otpCode.Should().NotBeNullOrEmpty();

            // Act - same OTP but without the MFA session id
            Result<AuthResponse> result = await _pb.Collection(collectionName)
                .AuthWithOtpAsync(otpRequest.Value.OtpId, otpCode);

            // Assert - MFA still pending, no token issued
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
        }
        finally
        {
            await CleanupAsync(collectionName, adminSession);
        }
    }

    private async Task<(string collectionName, string email)> SetupMfaCollectionAsync()
    {
        await _pb.Admins.AuthWithPasswordAsync(
            _fixture.Settings.AdminTesterEmail,
            _fixture.Settings.AdminTesterPassword);

        // Configure SMTP to use MailHog (needed to deliver the OTP 2nd factor)
        Result smtp = await _pb.Settings.UpdateAsync(new
        {
            smtp = new
            {
                enabled = true,
                host = "localhost",
                port = 1027,
                tls = false
            }
        });
        smtp.IsSuccess.Should().BeTrue();

        string id = $"{Guid.NewGuid():N}"[..6];
        string collectionName = $"mfa_auth_{id}";

        Result<CollectionModel> collection = await _pb.Collections.CreateAsync<CollectionModel>(new
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
                // MFA requires at least 2 enabled auth methods -> password + OTP
                ["passwordAuth"] = new Dictionary<string, object?>
                {
                    ["enabled"] = true,
                    ["identityFields"] = new[] { "email" }
                },
                ["otp"] = new Dictionary<string, object?>
                {
                    ["enabled"] = true
                },
                ["mfa"] = new Dictionary<string, object?>
                {
                    ["enabled"] = true,
                    ["duration"] = 600
                }
            }
        });
        collection.IsSuccess.Should().BeTrue();

        string email = $"mfa_{id}@example.com";
        Result<RecordResponse> user = await _pb.Collection(collectionName)
            .CreateAsync<RecordResponse>(new
            {
                email,
                password = "password123",
                passwordConfirm = "password123",
                verified = true
            });
        user.IsSuccess.Should().BeTrue();

        return (collectionName, email);
    }

    private async Task CleanupAsync(string collectionName, AuthResponse? adminSession)
    {
        if (adminSession != null)
        {
            _pb.AuthStore.Save(adminSession);
            await _pb.Collections.DeleteAsync(collectionName);
        }

        try { await _mailHogService.ClearAllMessagesAsync(); } catch (Exception) { }
    }
}