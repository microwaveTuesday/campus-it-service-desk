using System.Net;
using System.Text;
using System.Text.Json;
using CampusIT.Api.Services;
using Xunit;
namespace CampusIT.Tests;
public class WeatherLocationTests
{
    sealed class Reply(string body,HttpStatusCode status=HttpStatusCode.OK):HttpMessageHandler
    {
        public Uri? Requested {get;private set;}
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct){Requested=request.RequestUri;return Task.FromResult(new HttpResponseMessage(status){Content=new StringContent(body,Encoding.UTF8,"application/json")});}
    }
    const string FullWeather="""
    {"timezone":"Asia/Manila","current":{"time":"2026-11-01T12:00","temperature_2m":28,"relative_humidity_2m":80,"apparent_temperature":31,"weather_code":61,"wind_speed_10m":12},"daily":{"time":["2026-11-01","2026-11-02","2026-11-03"],"temperature_2m_max":[30,31,29],"temperature_2m_min":[24,25,23],"weather_code":[61,2,95],"precipitation_probability_max":[70,20,90]}}
    """;
    [Theory][InlineData(13.62,123.19,true)][InlineData(-90,-180,true)][InlineData(90,180,true)][InlineData(91,0,false)][InlineData(0,181,false)][InlineData(double.NaN,0,false)][InlineData(0,double.PositiveInfinity,false)]
    public void Coordinates_are_bounded_and_finite(double latitude,double longitude,bool expected)=>Assert.Equal(expected,WeatherService.ValidCoordinates(latitude,longitude));
    [Fact]public async Task Selected_coordinates_are_rounded_and_sent_to_fixed_forecast_host()
    {
        var handler=new Reply(FullWeather);var service=new WeatherService(new HttpClient(handler));
        var result=JsonSerializer.SerializeToElement(await service.Get(default,13.13906,123.7438));
        Assert.Equal("api.open-meteo.com",handler.Requested!.Host);Assert.Contains("latitude=13.14",handler.Requested.Query);Assert.Contains("longitude=123.74",handler.Requested.Query);Assert.Contains("timezone=auto",handler.Requested.Query);
        Assert.True(result.GetProperty("available").GetBoolean());Assert.Equal(3,result.GetProperty("forecast").GetArrayLength());Assert.Equal(70,result.GetProperty("forecast")[0].GetProperty("rainChance").GetInt32());Assert.Equal("Light rain",result.GetProperty("condition").GetString());
    }
    [Theory][InlineData(0,"Clear sky")][InlineData(2,"Partly cloudy")][InlineData(61,"Light rain")][InlineData(95,"Thunderstorms")][InlineData(999,"Conditions unavailable")]
    public void Weather_codes_have_readable_labels(int code,string expected)=>Assert.Equal(expected,WeatherService.Condition(code));
    [Theory][InlineData("\"relative_humidity_2m\":80","\"relative_humidity_2m\":101")][InlineData("\"temperature_2m_min\":[24,25,23]","\"temperature_2m_min\":[24]")][InlineData("\"precipitation_probability_max\":[70,20,90]","\"precipitation_probability_max\":[101,20,90]")][InlineData("\"2026-11-02\"","\"invalid-date\"")]
    public async Task Invalid_current_or_forecast_values_return_safe_fallback(string from,string to)
    {
        var result=JsonSerializer.SerializeToElement(await new WeatherService(new HttpClient(new Reply(FullWeather.Replace(from,to)))).Get(default));Assert.False(result.GetProperty("available").GetBoolean());
    }
    [Fact]public async Task City_search_returns_valid_locations_and_escapes_the_query()
    {
        var handler=new Reply("""{"results":[{"name":"Legazpi","admin1":"Bicol","country":"Philippines","latitude":13.13906,"longitude":123.7438}]}""");
        var result=JsonSerializer.SerializeToElement(await new WeatherService(new HttpClient(handler)).Search("Legazpi & Philippines",default));
        Assert.Equal("geocoding-api.open-meteo.com",handler.Requested!.Host);Assert.Contains("%26",handler.Requested.Query);Assert.Equal("Legazpi",result.GetProperty("locations")[0].GetProperty("name").GetString());
    }
    [Theory][InlineData("{\"results\":[{\"name\":\"Invalid\",\"latitude\":999,\"longitude\":0}]}")][InlineData("{\"results\":\"invalid\"}")][InlineData("not json")]
    public async Task Malformed_city_results_return_safe_fallback(string body)
    {
        var result=JsonSerializer.SerializeToElement(await new WeatherService(new HttpClient(new Reply(body))).Search("Legazpi",default));Assert.False(result.GetProperty("available").GetBoolean());Assert.Empty(result.GetProperty("locations").EnumerateArray());
    }
    [Fact]public async Task No_city_matches_returns_empty_available_result()
    {
        var result=JsonSerializer.SerializeToElement(await new WeatherService(new HttpClient(new Reply("{}"))).Search("Unknown town",default));Assert.True(result.GetProperty("available").GetBoolean());Assert.Empty(result.GetProperty("locations").EnumerateArray());
    }
}
