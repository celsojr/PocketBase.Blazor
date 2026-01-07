namespace PocketBase.Blazor.IntegrationTests.Clients.Collections;

using System.Net;
using Blazor.Models.Collection;
using Blazor.Models.Collection.Fields;
using Blazor.Responses;

[Collection("PocketBase.Blazor.Admin")]
public class DeleteTest
{
    private readonly IPocketBase _pb;
    private readonly PocketBaseAdminFixture _fixture;

    public DeleteTest(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
        _fixture = fixture;
    }

    [Fact]
    public async Task Delete_collection_successfully()
    {
        var createResult = await _pb.Collections.CreateAsync(
            new BaseCollectionCreateModel
            {
                Name = "exampleToDelete",
                Fields = new List<FieldModel>
                {
                    new TextFieldModel
                    {
                        Name = "title",
                        Required = true
                    }
                }
            });

        createResult.IsSuccess.Should().BeTrue();
        var deleteResult = await _pb.Collections.DeleteAsync(createResult.Value.Id);
        deleteResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_collection_should_fail_when_not_admin()
    {
        var client = new PocketBase(_fixture.Settings.BaseUrl);

        var authResult = await client.Collection("users")
            .AuthWithPasswordAsync(
                _fixture.Settings.UserTesterEmail,
                _fixture.Settings.UserTesterPassword
             );

        authResult.IsSuccess.Should().BeTrue();

        var createResult = await _pb.Collections.CreateAsync(
            new BaseCollectionCreateModel
            {
                Name = "exampleToDelete",
                DeleteRule = "@request.auth.id != \"\"",
                Fields = new List<FieldModel>
                {
                    new TextFieldModel
                    {
                        Name = "title",
                        Required = true
                    }
                }
            });

        createResult.IsSuccess.Should().BeTrue();

        var deleteResult = await client.Collections.DeleteAsync(createResult.Value.Id);

        deleteResult.IsSuccess.Should().BeFalse();
        deleteResult.Errors.Should().NotBeNull();

        var error = JsonSerializer.Deserialize<ErrorResponse>(deleteResult.Errors[0].Message);

        error.Should().NotBeNull();
        error.Message.Should().Be("The authorized record is not allowed to perform this action.");
        error.Status.Should().Be((int)HttpStatusCode.Forbidden);
    }
}

