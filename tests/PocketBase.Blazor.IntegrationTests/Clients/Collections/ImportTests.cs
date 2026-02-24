namespace PocketBase.Blazor.IntegrationTests.Clients.Collections;

using Blazor.Models;
using Blazor.Models.Collection;
using Blazor.Models.Collection.Fields;

[Trait("Category", "Integration")]
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
        string baseCollectionName = $"import_base_{Guid.NewGuid():N}"[..20];
        string viewCollectionName = $"import_view_{Guid.NewGuid():N}"[..20];

        CollectionCreateModel[] collections =
        [
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
        ];

        Result result = await _pb.Collections.ImportAsync(collections);

        result.IsSuccess.Should().BeTrue();

        Result<CollectionModel> baseGet = await _pb.Collections.GetOneAsync<CollectionModel>(baseCollectionName);
        baseGet.IsSuccess.Should().BeTrue();

        Result<CollectionModel> viewGet = await _pb.Collections.GetOneAsync<CollectionModel>(viewCollectionName);
        viewGet.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Import_is_idempotent()
    {
        string name = $"import_idempotent_{Guid.NewGuid():N}"[..20];

        CollectionCreateModel[] collections =
        [
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
        ];

        Result first = await _pb.Collections.ImportAsync(collections);
        Result second = await _pb.Collections.ImportAsync(collections);

        first.IsSuccess.Should().BeTrue();
        second.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Import_without_deleteMissing_preserves_existing_collections()
    {
        string existingName = $"existing_{Guid.NewGuid():N}"[..20];

        await _pb.Collections.CreateAsync(
            new BaseCollectionCreateModel
            {
                Name = existingName,
                Fields = []
            }
        );

        Result importResult = await _pb.Collections.ImportAsync(
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

        Result<CollectionModel> getExisting = await _pb.Collections.GetOneAsync<CollectionModel>(existingName);
        getExisting.IsSuccess.Should().BeTrue();
    }

    [Fact(Skip = "Destructive test - run only in isolated environment")]
    public async Task Import_with_deleteMissing_removes_non_imported_collections()
    {
        // **This test is destructive — isolate it carefully**
        string toDelete = $"delete_me_{Guid.NewGuid():N}"[..20];

        await _pb.Collections.CreateAsync(
            new BaseCollectionCreateModel
            {
                Name = toDelete,
                Fields = []
            }
        );

        Result importResult = await _pb.Collections.ImportAsync(
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

        Result<CollectionModel> getDeleted = await _pb.Collections.GetOneAsync<CollectionModel>(toDelete);
        getDeleted.IsSuccess.Should().BeFalse();
    }
}
