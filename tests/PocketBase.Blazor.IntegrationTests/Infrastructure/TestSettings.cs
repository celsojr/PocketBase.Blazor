namespace PocketBase.Blazor.IntegrationTests.Infrastructure;

public sealed class TestSettings
{
    public string BaseUrl { get; init; } = "http://127.0.0.1:8092";
    public string TestUserEmail { get; init; } = "user_tester@email.com";
    public string TestUserPassword { get; init; } = "Nyp9wiGaAC4qGWz";
}
