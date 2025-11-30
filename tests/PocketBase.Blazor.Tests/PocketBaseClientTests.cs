using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;

namespace PocketBase.Blazor.Tests;

public class PocketBaseClientTests
{
    private PocketBaseClient CreateClient(HttpResponseMessage response)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
               "SendAsync",
               ItExpr.IsAny<HttpRequestMessage>(),
               ItExpr.IsAny<CancellationToken>()
           )
           .ReturnsAsync(response);

        var http = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://fakeapi.local")
        };

        var options = new PocketBaseOptions();
        return new PocketBaseClient(http, options);
    }

    [Fact]
    public async Task GetRecordAsync_ReturnsValue_WhenSuccess()
    {
        var fakeRecord = new Post { Id = "1", Title = "Hello" };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(fakeRecord)
        };

        var client = CreateClient(response);

        var (value, error) = await client.GetRecordAsync<Post>("posts", "1");

        Assert.NotNull(value);
        Assert.Equal("1", value.Id);
        Assert.Equal("Hello", value.Title);
        Assert.Null(error);
    }

    [Fact]
    public async Task GetRecordAsync_ReturnsError_WhenForbidden()
    {
        var errorResponse = new
        {
            status = 403,
            message = "Only superusers can access this action.",
            data = new { }
        };

        var response = new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = JsonContent.Create(errorResponse)
        };

        var client = CreateClient(response);

        var (value, error) = await client.GetRecordAsync<dynamic>("posts", "1");

        Assert.Null(value);
        Assert.NotNull(error);
        Assert.Equal(403, error.Status);
        Assert.Contains("superusers", error.Message);
        Assert.True(error.RequiresApiKey);
    }

    [Fact]
    public async Task GetListAsync_ReturnsItems_WhenSuccess()
    {
        var fakeList = new
        {
            Items = new[]
            {
                new Post { Id = "1", Title = "Post1" },
                new Post { Id = "2", Title = "Post2" }
            }
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(fakeList)
        };

        var client = CreateClient(response);

        var (items, error) = await client.GetListAsync<Post>("posts");

        Assert.Null(error);
        Assert.Equal(2, items.Count);
        Assert.Equal("Post1", items[0].Title);
    }

    [Fact]
    public async Task CreateRecordAsync_ReturnsValue_WhenSuccess()
    {
        var newPost = new Post { Id = "10", Title = "New Post" };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(newPost)
        };

        var client = CreateClient(response);

        var (value, error) = await client.CreateRecordAsync<Post>("posts", new { Title = "New Post" });

        Assert.Null(error);
        Assert.NotNull(value);
        Assert.Equal("10", value.Id);
        Assert.Equal("New Post", value.Title);
    }

}

public class Post
{
    public string Id { get; set; } = "";
    public string? Title { get; set; }
}

