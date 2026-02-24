namespace PocketBase.Blazor.IntegrationTests.Clients.Record;

using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Blazor.Models;
using Blazor.Requests.Auth;
using Blazor.Responses;
using Blazor.Responses.Auth;
using Microsoft.Playwright;

[Trait("Category", "Integration")]
[Trait("Requires", "Playwright")]
[Collection("PocketBase.Blazor.Admin")]
public class AuthWithOAuth2RecordTests : IAsyncLifetime
{
    private readonly IPocketBase _pb;
    private readonly IPlaywright _playwright;
    private readonly IBrowser _browser;
    private const string CollectionName = "test_oauth2_users";
    private const string TestEmail = "oauth2_test@example.com";
    private const string TestPassword = "Test123456!";

    // IMPORTANT: DO NOT USE THIS URL
    // This endpoint is reserved for PocketBase's internal OAuth2 handling
    // private const string RedirectUrl = "http://localhost:8092/api/oauth2-redirect";

    // Use a custom redirect URL instead
    // The URL doesn't need to be a real endpoint - it just needs to:
    //   1. Match what's registered in your OAuth2 provider's dashboard
    //   2. Allow us to capture the authorization code from the URL
    private const string RedirectUrl = "http://localhost:8092/test-oauth-callback";

    private readonly string? _testGoogleEmail =
        Environment.GetEnvironmentVariable("GOOGLE_TEST_EMAIL") ?? "valid_google_id@gmail.com";

    private readonly string? _testGooglePassword =
        Environment.GetEnvironmentVariable("GOOGLE_TEST_PASSWORD") ?? "valid_password@change123!";

    public AuthWithOAuth2RecordTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
        _playwright = Playwright.CreateAsync().GetAwaiter().GetResult();
        _browser = _playwright.Chromium
            .LaunchAsync(new BrowserTypeLaunchOptions {
                Headless = false
            }).GetAwaiter().GetResult();
    }

    public async Task InitializeAsync()
    {
        // Create auth collection with OAuth2 enabled
        Result<RecordResponse> collectionResult = await _pb.Collections.CreateAsync<RecordResponse>(new
        {
            name = CollectionName,
            type = "auth",
            schema = new[]
            {
                new { name = "email", type = "email", required = true, unique = true }
            }
        }, new CommonOptions
        {
            Body = new Dictionary<string, object?>
            {
                ["oauth2"] = new Dictionary<string, object?>
                {
                    ["enabled"] = true,
                    ["providers"] = new[]
                    {
                        new
                        {
                            name = "google",
                            clientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") ?? "00000000-dummy.apps.googleusercontent.com",
                            clientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") ?? "GOCSPX-dummy"
                        }
                    }
                }
            }
        });

        // Ensure the collection is created
        collectionResult.IsSuccess.Should().BeTrue();

        // Create test user for this collection
        await _pb.Collection(CollectionName)
            .CreateAsync<RecordResponse>(new
            {
                email = TestEmail,
                password = TestPassword,
                passwordConfirm = TestPassword
            });
    }

    public async Task DisposeAsync()
    {
        // Clean up PocketBase collection
        Result<ListResult<CollectionModel>> listResult = await _pb.Collections
            .GetListAsync<CollectionModel>(options: new ListOptions { SkipTotal = true });

        listResult.IsSuccess.Should().BeTrue();

        CollectionModel? collection = listResult.Value.Items
            .FirstOrDefault(c => c.Name?.Equals(CollectionName) == true);

        if (collection?.Id != null)
        {
            await _pb.Collections.DeleteAsync(collection.Id);
        }

        await _browser.DisposeAsync();
        _playwright.Dispose();
    }

    [Fact]
    [Trait("Requires", "OAuth2")]
    public async Task AuthWithOAuth2CodeAsync_WithValidRequest_ReturnsSuccess()
    {
        // Step 1: First get auth methods to get OAuth2 provider info
        Result<AuthMethodsResponse> authMethods = await _pb.Collection(CollectionName)
            .ListAuthMethodsAsync();

        ProviderResponse? googleProvider = authMethods.Value?.Oauth2?.Providers?
            .FirstOrDefault(p => p.Name == "google");

        if (googleProvider == null)
        {
            return; // Skip if no provider configured
        }

        googleProvider.CodeVerifier.Should().NotBeNull();

        // Store the provider with its code verifier
        string storedCodeVerifier = googleProvider!.CodeVerifier;
        string authUrl = googleProvider.AuthURL + RedirectUrl;

        // Step 2: Use Playwright to automate Google login
        IPage page = await _browser.NewPageAsync();

        try
        {
            // Step 3: Wait for redirect back to our redirect URL and capture the code
            Task<string> codeTask = WaitForOAuthRedirectAndCaptureCode(page);

            // Ensure the required values are present
            authUrl.Should().NotBeNullOrEmpty();
            _testGoogleEmail.Should().NotBeNullOrEmpty();
            _testGooglePassword.Should().NotBeNullOrEmpty();

            // Navigate to Google's auth URL
            await page.GotoAsync(authUrl);

            // Wait for email input and fill it
            await page.Locator("input[type=\"email\"]").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
            await page.Locator("input[type=\"email\"]").FillAsync(_testGoogleEmail);
            await page.Locator("#identifierNext").ClickAsync();
        
            // Wait for password input and fill it
            await page.Locator("input[type=\"password\"]").First.WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
            await page.Locator("input[type=\"password\"]").First.FillAsync(_testGooglePassword);
            await page.Locator("#passwordNext").ClickAsync();

            // Handle consent screen if it appears
            try
            {
                await page.Locator("button:has-text('Continue')").ClickAsync(new LocatorClickOptions
                { 
                    Timeout = 5000
                });
            }
            catch { /* Consent screen might not appear */ }

            // Note: The browser will show a 404 error when redirected here,
            // but that's fine - we only need to extract the 'code' parameter from the URL

            // Step 4: Wait for the code from redirect
            string code = await codeTask.WaitAsync(TimeSpan.FromSeconds(30));
            code.Should().NotBeNullOrEmpty();

            AuthWithOAuth2Request request = new AuthWithOAuth2Request
            {
                Provider = "google",
                Code = code,
                CodeVerifier = storedCodeVerifier,
                RedirectUrl = RedirectUrl,
                CreateData = new
                {
                    email = TestEmail
                }
            };

            Result<AuthRecordResponse> result = await _pb.Collection(CollectionName)
                .AuthWithOAuth2CodeAsync(request);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Record.Should().NotBeNull();
            result.Value.Token.Should().NotBeNullOrEmpty();
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task AuthWithOAuth2CodeAsync_WithInvalidProvider_ReturnsFailure()
    {
        AuthWithOAuth2Request request = new AuthWithOAuth2Request
        {
            Provider = "invalid_provider",
            Code = "test_code",
            CodeVerifier = "test_verifier",
            RedirectUrl = RedirectUrl
        };

        Result<AuthRecordResponse> result = await _pb.Collection(CollectionName)
            .AuthWithOAuth2CodeAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeNull();
    }

    [Fact]
    [Trait("Requires", "OAuth2")]
    public async Task AuthWithOAuth2CodeAsync_WithInvalidCode_ReturnsFailure()
    {
        Result<AuthMethodsResponse> authMethods = await _pb.Collection(CollectionName)
            .ListAuthMethodsAsync();

        ProviderResponse? googleProvider = authMethods.Value?.Oauth2?.Providers?
            .FirstOrDefault(p => p.Name == "google");

        AuthWithOAuth2Request request = new AuthWithOAuth2Request
        {
            Provider = "google",
            Code = "invalid_code",
            CodeVerifier = googleProvider?.CodeVerifier ?? "test_verifier",
            RedirectUrl = RedirectUrl
        };

        Result<AuthRecordResponse> result = await _pb.Collection(CollectionName)
            .AuthWithOAuth2CodeAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeNull();
    }

    [Fact]
    public async Task AuthWithOAuth2CodeAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _pb.Collection(CollectionName)
                .AuthWithOAuth2CodeAsync(null!));
    }

    private static async Task<string> WaitForOAuthRedirectAndCaptureCode(IPage page)
    {
        TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
    
        page.RequestFinished += (_, request) =>
        {
            if (request.Url.Contains(RedirectUrl))
            {
                Match codeMatch = Regex.Match(request.Url, @"code=([^&]+)");
                if (codeMatch.Success)
                {
                    tcs.TrySetResult(Uri.UnescapeDataString(codeMatch.Groups[1].Value));
                }
            }
        };
    
        _ = Task.Run(async () =>
        {
            while (!tcs.Task.IsCompleted)
            {
                string currentUrl = page.Url;
                if (currentUrl.Contains(RedirectUrl))
                {
                    Match codeMatch = Regex.Match(currentUrl, @"code=([^&]+)");
                    if (codeMatch.Success)
                    {
                        tcs.TrySetResult(Uri.UnescapeDataString(codeMatch.Groups[1].Value));
                        break;
                    }
                }
                await Task.Delay(100);
            }
        });
    
        return await tcs.Task;
    }
}
