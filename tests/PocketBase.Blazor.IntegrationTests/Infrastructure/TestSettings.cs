namespace PocketBase.Blazor.IntegrationTests.Infrastructure;

public sealed class TestSettings
{
    public string BaseUrl { get; init; } = "http://127.0.0.1:8092";
    public string UserTesterEmail { get; init; } = "user_tester@email.com";
    public string UserTesterPassword { get; init; } = "Nyp9wiGaAC4qGWz";
    public string AdminTesterEmail { get; init; } = "admin_tester@email.com";
    public string AdminTesterPassword { get; init; } = "gDxTCmnq7K8xyKn";
}

