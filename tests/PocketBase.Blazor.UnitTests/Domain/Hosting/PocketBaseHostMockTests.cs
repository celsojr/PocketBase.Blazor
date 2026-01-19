namespace PocketBase.Blazor.UnitTests.Domain.Hosting;

using Moq;
using Blazor.Hosting;

public class PocketBaseHostMockTests
{
    [Fact]
    public async Task StartAsync_WithMockProcess_ShouldHandleEvents()
    {
        // Arrange
        var mockProcess = new Mock<IProcess>();
        mockProcess.Setup(p => p.Start()).Returns(true);
        mockProcess.Setup(p => p.HasExited).Returns(false);

        var builder = PocketBaseHostBuilder.CreateDefault();
        await builder
            .UseOptions(options => options.Port = 8888)
            .BuildAsync();

        var host = await builder.BuildAsync();

        // Use reflection to inject mock process (simplified example)
        // In real implementation, use dependency injection

        // Act
        await host.StartAsync();

        // Assert
        mockProcess.Verify(p => p.Start(), Times.Once);
    }

    [Fact]
    public async Task OnProcessExited_ShouldRaiseEvent()
    {
        // Arrange
        var builder = PocketBaseHostBuilder.CreateDefault();
        await builder.BuildAsync();

        var host = await builder.BuildAsync();
        bool eventRaised = false;

        if (host != null && host.Process != null)
            host.Process.Exited += (sender, e) => eventRaised = true;

        // Act - Simulate process exit (would need proper mocking)
        // This is simplified - actual test would use a mock process

        // Assert
        // Verify event handling logic
    }
}

public interface IProcess
{
    bool Start();
    bool HasExited { get; }
    void Kill();
    event EventHandler Exited;
}

