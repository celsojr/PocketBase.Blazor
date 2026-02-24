namespace PocketBase.Blazor.IntegrationTests.Clients.Record;

using Blazor.Models;
using Blazor.Responses.Auth;

[Trait("Category", "Integration")]
[Collection("PocketBase.Blazor.Admin")]
public class ListAuthMethodsRecordTests : IAsyncLifetime
{
    private readonly IPocketBase _pb;
    private const string CollectionName = "test_auth_listmethods";

    public ListAuthMethodsRecordTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
    }

    public async Task InitializeAsync()
    {
        // Create auth collection with OTP enabled
        await _pb.Collections.CreateAsync<CollectionModel>(new
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
                ["otp"] = new Dictionary<string, object?>
                {
                    ["enabled"] = true
                }
            }
        });
    }

    public async Task DisposeAsync()
    {
        // Clean up
        Result<ListResult<CollectionModel>> listResult = await _pb.Collections
            .GetListAsync<CollectionModel>(options: new ListOptions { SkipTotal = true });

        listResult.IsSuccess.Should().BeTrue();

        CollectionModel? collection = listResult.Value.Items
            .FirstOrDefault(c => c.Name?.Equals(CollectionName) == true);

        if (collection?.Id != null)
        {
            await _pb.Collections.DeleteAsync(collection.Id);
        }
    }

    [Fact]
    public async Task ListAuthMethodsAsync_ReturnsSuccessResult()
    {
        Result<AuthMethodsResponse> result = await _pb.Collection(CollectionName).ListAuthMethodsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task ListAuthMethodsAsync_ReturnsAuthMethodsWithExpectedStructure()
    {
        Result<AuthMethodsResponse> result = await _pb.Collection(CollectionName).ListAuthMethodsAsync();

        result.Value.Should().NotBeNull();
        result.Value.Password.Should().NotBeNull();
        result.Value.Oauth2.Should().NotBeNull();
        result.Value.Mfa.Should().NotBeNull();
        result.Value.Otp.Should().NotBeNull();
    }

    [Fact]
    public async Task ListAuthMethodsAsync_PasswordAuth_HasEnabledAndIdentityFields()
    {
        Result<AuthMethodsResponse> result = await _pb.Collection(CollectionName).ListAuthMethodsAsync();

        result.Value.Password.IdentityFields.Should().NotBeNull();
    }

    [Fact]
    public async Task ListAuthMethodsAsync_OAuth2_ReturnsProviders()
    {
        Result<AuthMethodsResponse> result = await _pb.Collection(CollectionName).ListAuthMethodsAsync();

        result.Value.Oauth2.Providers.Should().NotBeNull();
    }

    [Fact]
    public async Task ListAuthMethodsAsync_Mfa_ReturnsEnabledAndDuration()
    {
        Result<AuthMethodsResponse> result = await _pb.Collection(CollectionName).ListAuthMethodsAsync();

        result.Value.Mfa.Duration.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task ListAuthMethodsAsync_Otp_ReturnsEnabledAndValidDuration()
    {
        Result<AuthMethodsResponse> result = await _pb.Collection(CollectionName).ListAuthMethodsAsync();

        result.Value.Otp.Enabled.Should().BeTrue(); // We set it to true
        result.Value.Otp.Duration.Should().BeGreaterThanOrEqualTo(10);
    }
}
