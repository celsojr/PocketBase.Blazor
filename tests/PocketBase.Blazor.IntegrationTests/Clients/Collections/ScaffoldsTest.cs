namespace PocketBase.Blazor.IntegrationTests.Clients.Collections;

[Collection("PocketBase.Blazor.Admin")]
public class ScaffoldsTest
{
    private readonly IPocketBase _pb;

    public ScaffoldsTest(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
    }

    [Fact]
    public async Task GetScaffolds_returns_all_expected_scaffold_types()
    {
        var result = await _pb.Collections.GetScaffoldsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var scaffolds = result.Value!;

        scaffolds.Auth.Should().NotBeNull();
        scaffolds.Base.Should().NotBeNull();
        scaffolds.View.Should().NotBeNull();

        scaffolds.Auth.Type.Should().Be("auth");
        scaffolds.Base.Type.Should().Be("base");
        scaffolds.View.Type.Should().Be("view");
    }

    [Fact]
    public async Task GetScaffolds_preserves_field_options()
    {
        var result = await _pb.Collections.GetScaffoldsAsync();
        var auth = result.Value!.Auth;

        auth.Fields.Should().NotBeEmpty();

        var idField = auth.Fields
            .First(f => f.Name == "id");

        idField.Type.Should().Be("text");
        idField.Required.Should().BeTrue();
        idField.System.Should().BeTrue();

        idField.Options.Should().ContainKey("min");
        idField.Options.Should().ContainKey("max");
        idField.Options.Should().ContainKey("pattern");

        idField.Options["min"].GetInt32().Should().Be(15);
        idField.Options["max"].GetInt32().Should().Be(15);
    }

    [Fact]
    public async Task GetScaffolds_auth_collection_contains_auth_configuration()
    {
        var result = await _pb.Collections.GetScaffoldsAsync();
        var auth = result.Value!.Auth;

        auth.AuthRule.Should().NotBeNull();
        auth.AuthAlert.Should().NotBeNull();
        auth.PasswordAuth.Should().NotBeNull();
        auth.AuthToken.Should().NotBeNull();

        auth.PasswordAuth!.Enabled.Should().BeTrue();
        auth.PasswordAuth.IdentityFields.Should().Contain("email");
    }

    [Fact]
    public async Task GetScaffolds_view_collection_contains_view_query()
    {
        var result = await _pb.Collections.GetScaffoldsAsync();
        var view = result.Value!.View;

        view.Type.Should().Be("view");
        view.ViewQuery.Should().NotBeNull();
        view.Fields.Should().BeEmpty();
    }

    [Fact]
    public async Task GetScaffolds_auth_indexes_are_present()
    {
        var result = await _pb.Collections.GetScaffoldsAsync();
        var auth = result.Value!.Auth;

        auth.Indexes.Should().NotBeEmpty();
        auth.Indexes.Any(i => i.Contains("email")).Should().BeTrue();
    }
}

