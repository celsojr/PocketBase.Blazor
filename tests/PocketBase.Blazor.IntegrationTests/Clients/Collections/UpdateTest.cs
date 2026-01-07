namespace PocketBase.Blazor.IntegrationTests.Clients.Collections;

using Blazor.Models.Collection;
using Blazor.Models.Collection.Fields;

[Collection("PocketBase.Blazor.Admin")]
public class UpdateTest
{
    private readonly IPocketBase _pb;
    private readonly PocketBaseAdminFixture _fixture;
    
    public UpdateTest(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
        _fixture = fixture;
    }

    [Fact]
    public async Task Update_collection_successfully()
    {
        var createResult = await _pb.Collections.CreateAsync(
            new BaseCollectionCreateModel
            {
                Name = "exampleToUpdate",
                Fields = new List<FieldModel>
                {
                    new TextFieldModel
                    {
                        Name = "title",
                        Required = true,
                        Min = 10
                    }
                }
            });

        createResult.IsSuccess.Should().BeTrue();
 
        var updateResult = await _pb.Collections.UpdateAsync(
            createResult.Value.Id,
            new BaseCollectionUpdateModel
            {
                Name = "exampleUpdated",
                Fields =
                {
                    new TextFieldModel
                    {
                        Name = "title",
                        Required = true,
                        Min = 5
                    },
                    new BoolFieldModel
                    {
                        Name = "isActive"
                    }
                }
            });

        updateResult.IsSuccess.Should().BeTrue();
    }
}

