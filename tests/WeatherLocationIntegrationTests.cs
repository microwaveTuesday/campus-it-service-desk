using System.Net;
using Xunit;
namespace CampusIT.Tests;
public class WeatherLocationIntegrationTests
{
    [Theory][InlineData("/api/weather?latitude=91&longitude=123")][InlineData("/api/weather?latitude=13")][InlineData("/api/weather?longitude=123")][InlineData("/api/weather?latitude=NaN&longitude=123")][InlineData("/api/weather/locations?query=a")][InlineData("/api/weather/locations")]
    public async Task Invalid_weather_query_is_rejected_before_calling_upstream(string path)
    {
        await using var database=await IsolatedAccountDatabase.Create();using var staff=await TestAccounts.Client(database.Factory);Assert.Equal(HttpStatusCode.BadRequest,(await staff.GetAsync(path)).StatusCode);
    }
    [Fact]public async Task City_search_requires_authentication()
    {
        await using var database=await IsolatedAccountDatabase.Create();using var client=database.Factory.CreateClient();Assert.Equal(HttpStatusCode.Unauthorized,(await client.GetAsync("/api/weather/locations?query=Legazpi")).StatusCode);
    }
}
