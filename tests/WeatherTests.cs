using System.Net;
using System.Text;
using CampusIT.Api.Services;
using Xunit;
namespace CampusIT.Tests;
public class WeatherTests
{
    sealed class Reply(string body, HttpStatusCode code) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
            => Task.FromResult(new HttpResponseMessage(code) { Content = new StringContent(body, Encoding.UTF8, "application/json") });
    }
    [Theory]
    [InlineData("{}")]
    [InlineData("not json")]
    [InlineData("{\"current\":{\"temperature_2m\":null}}")]
    [InlineData("{\"current\":{\"temperature_2m\":\"hot\"}}")]
    [InlineData("{\"current\":{\"temperature_2m\":999}}")]
    public async Task Malformed_upstream_returns_fallback(string body)
    {
        var service = new WeatherService(new HttpClient(new Reply(body, HttpStatusCode.OK)));
        var json = System.Text.Json.JsonSerializer.Serialize(await service.Get(default));
        Assert.Contains("\"available\":false", json);
    }
    [Fact] public async Task Upstream_failure_returns_fallback()
    {
        var service = new WeatherService(new HttpClient(new Reply("", HttpStatusCode.ServiceUnavailable)));
        Assert.Contains("\"available\":false", System.Text.Json.JsonSerializer.Serialize(await service.Get(default)));
    }
    [Fact] public async Task Valid_upstream_returns_temperature()
    {
        var service = new WeatherService(new HttpClient(new Reply("{\"current\":{\"temperature_2m\":27}}", HttpStatusCode.OK)));
        Assert.Contains("\"temperatureC\":27", System.Text.Json.JsonSerializer.Serialize(await service.Get(default)));
    }
}
