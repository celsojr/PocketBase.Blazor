namespace PocketBase.Blazor.IntegrationTests.Clients.Collections;

using Blazor.Models;
using Blazor.Models.Collection;
using Blazor.Models.Collection.Fields;

[Collection("PocketBase.Blazor.Admin")]
public class ImportTests
{
    private readonly IPocketBase _pb;
    private readonly PocketBaseAdminFixture _fixture;

    public ImportTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
        _fixture = fixture;
    }

    [Fact]
    public async Task Import_collections_successfully()
    {
        var baseCollectionName = $"import_base_{Guid.NewGuid():N}"[..20];
        var viewCollectionName = $"import_view_{Guid.NewGuid():N}"[..20];

        var collections = new CollectionCreateModel[]
        {
            new BaseCollectionCreateModel
            {
                Name = baseCollectionName,
                Fields =
                [
                    new TextFieldModel
                    {
                        Name = "title",
                        Required = true,
                        Min = 3
                    }
                ]
            },
            new ViewCollectionCreateModel
            {
                Name = viewCollectionName,
                ViewQuery = $"SELECT id, title FROM {baseCollectionName};",
            }
        };

        var result = await _pb.Collections.ImportAsync(collections);

        result.IsSuccess.Should().BeTrue();

        var baseGet = await _pb.Collections.GetOneAsync<CollectionModel>(baseCollectionName);
        baseGet.IsSuccess.Should().BeTrue();

        var viewGet = await _pb.Collections.GetOneAsync<CollectionModel>(viewCollectionName);
        viewGet.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Import_is_idempotent()
    {
        var name = $"import_idempotent_{Guid.NewGuid():N}"[..20];

        var collections = new CollectionCreateModel[]
        {
            new BaseCollectionCreateModel
            {
                Name = name,
                Fields =
                [
                    new BoolFieldModel
                    {
                        Name = "enabled",
                        Default = true
                    }
                ]
            }
        };

        var first = await _pb.Collections.ImportAsync(collections);
        var second = await _pb.Collections.ImportAsync(collections);

        first.IsSuccess.Should().BeTrue();
        second.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Import_without_deleteMissing_preserves_existing_collections()
    {
        var existingName = $"existing_{Guid.NewGuid():N}"[..20];

        await _pb.Collections.CreateAsync(
            new BaseCollectionCreateModel
            {
                Name = existingName,
                Fields = []
            }
        );

        var importResult = await _pb.Collections.ImportAsync(
            collections:
            [
                new BaseCollectionCreateModel
                {
                    Name = $"another_{Guid.NewGuid():N}"[..20],
                    Fields =
                    [
                        new BoolFieldModel
                        {
                            Name = "enabled",
                            Default = true
                        }
                    ]
                }
            ],
            deleteMissing: false // This is false by default here on PocketBase.Blazor
        );

        importResult.IsSuccess.Should().BeTrue();

        var getExisting = await _pb.Collections.GetOneAsync<CollectionModel>(existingName);
        getExisting.IsSuccess.Should().BeTrue();
    }

    [Fact(Skip = "Destructive test - run only in isolated environment")]
    public async Task Import_with_deleteMissing_removes_non_imported_collections()
    {
        // **This test is destructive — isolate it carefully**
        var toDelete = $"delete_me_{Guid.NewGuid():N}"[..20];

        await _pb.Collections.CreateAsync(
            new BaseCollectionCreateModel
            {
                Name = toDelete,
                Fields = []
            }
        );

        var importResult = await _pb.Collections.ImportAsync(
            collections:
            [
                new BaseCollectionCreateModel
                {
                    Name = $"another_{Guid.NewGuid():N}"[..20],
                    Fields =
                    [
                        new BoolFieldModel
                        {
                            Name = "enabled",
                            Default = true
                        }
                    ]
                }
            ],
            deleteMissing: true // This permanently deletes all other tables.
        );

        importResult.IsSuccess.Should().BeTrue();

        var getDeleted = await _pb.Collections.GetOneAsync<CollectionModel>(toDelete);
        getDeleted.IsSuccess.Should().BeFalse();
    }
}

