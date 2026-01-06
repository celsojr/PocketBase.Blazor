namespace PocketBase.Blazor.IntegrationTests.Clients.Collections;

using Blazor.Models;
using Blazor.Models.Collection;

[Collection("PocketBase.Blazor.Admin")]
public class CreateTest
{
    private readonly IPocketBase _pb;

    public CreateTest(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
    }

    [Fact]
    public async Task Create_base_collections_successfully()
    {
        var baseResult = await _pb.Collections.CreateAsync(
            new BaseCollectionCreateModel
            {
                Name = "exampleBase",
                Fields =
                {
                    new TextFieldModel
                    {
                        Name = "title",
                        Required = true,
                        Min = 10
                    },
                    new BoolFieldModel
                    {
                        Name = "status"
                    }
                }
            });

        baseResult.IsSuccess.Should().BeTrue();
        baseResult.Value.Name.Should().Be("exampleBase");
    }

    [Fact]
    public async Task Create_auth_collection_successfully()
    {
        var newCollection = new AuthCollectionCreateModel
        {
            Name = "exampleAuth",
            CreateRule = "id = @request.auth.id",
            UpdateRule = "id = @request.auth.id",
            DeleteRule = "id = @request.auth.id",
            Fields =
            {
                new TextFieldModel
                {
                    Name = "name"
                }
            },
            PasswordAuth =
            {
                // Enabled = true,
                IdentityFields = { "email" }
            }
        };

        var createResult = await _pb.Collections.CreateAsync(newCollection);
        createResult.IsSuccess.Should().BeTrue();
        createResult.Value.Name.Should().Be(newCollection.Name);
        createResult.Value.CreateRule.Should().Be(newCollection.CreateRule);
        createResult.Value.UpdateRule.Should().Be(newCollection.UpdateRule);
        createResult.Value.DeleteRule.Should().Be(newCollection.DeleteRule);
    }

    [Fact]
    public async Task Create_view_collection_successfully()
    {
        var newCollection = new ViewCollectionCreateModel
        {
            Name = "exampleView",
            ListRule = "@request.auth.id != \"\"",
            ViewQuery = "SELECT id, name FROM posts"
        };

        var createResult = await _pb.Collections.CreateAsync(newCollection);
        createResult.IsSuccess.Should().BeTrue();
        createResult.Value.Name.Should().Be(newCollection.Name);
        createResult.Value.ListRule.Should().Be(newCollection.ListRule);
        createResult.Value.ViewQuery.Should().Be(newCollection.ViewQuery);
    }

    [Fact]
    public async Task Create_collection_using_plain_object_payload()
    {
        var name = $"anon_{Guid.NewGuid():N}"[..20];

        var request = new
        {
            name = name,
            type = "base",
            fields = new object[]
            {
                new
                {
                    name = "title",
                    type = "text",
                    required = true,
                    min = 10
                },
                new
                {
                    name = "status",
                    type = "bool"
                }
            }
        };

        var result = await _pb.Collections.CreateAsync<CollectionModel>(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
    }
}

