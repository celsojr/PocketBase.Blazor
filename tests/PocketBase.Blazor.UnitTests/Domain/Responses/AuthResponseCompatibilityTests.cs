namespace PocketBase.Blazor.UnitTests.Domain.Responses;

using System.Text.Json;
using Blazor.Responses.Auth;
using FluentAssertions;

[Trait("Category", "Unit")]
public class AuthResponseCompatibilityTests
{
    [Fact]
    public void AuthRecordResponse_ShouldBeAssignableToAuthResponse()
    {
        AuthRecordResponse authRecord = new AuthRecordResponse
        {
            Token = "jwt",
            Record = new UserResponse
            {
                Id = "user_1",
                Email = "user@example.com",
            },
            Meta = new AuthMetaResponse(),
        };

        AuthResponse asBase = authRecord;

        asBase.Token.Should().Be("jwt");
        asBase.Record.Should().NotBeNull();
        authRecord.Meta.Should().NotBeNull();
    }

    [Fact]
    public void AuthRecordResponse_Deserialization_ShouldPopulateBaseAndMetaProperties()
    {
        const string json = """
            {
              "token": "jwt_token",
              "record": {
                "id": "user_1",
                "email": "user@example.com",
                "verified": true,
                "emailVisibility": false
              },
              "meta": {
                "name": "github",
                "avatarURL": "https://example.com/avatar.png"
              }
            }
            """;

        AuthRecordResponse? response = JsonSerializer.Deserialize<AuthRecordResponse>(
            json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });

        response.Should().NotBeNull();
        response!.Token.Should().Be("jwt_token");
        response.Record.Should().NotBeNull();
        response.Record!.Email.Should().Be("user@example.com");
        response.Meta.Should().NotBeNull();
    }
}

