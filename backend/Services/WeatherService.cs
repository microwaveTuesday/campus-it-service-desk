using System.Globalization;
using System.Text.Json;
namespace CampusIT.Api.Services;

public record WeatherDay(string date,double maxTemperatureC,double minTemperatureC,int? rainChance,string condition);
public record WeatherPlace(string name,string region,string country,double latitude,double longitude);
public class WeatherService(HttpClient client)
{
    public static bool ValidTemp(double? value)=>value is >= -100 and <= 70;
    public static bool ValidCoordinates(double latitude,double longitude)=>double.IsFinite(latitude)&&double.IsFinite(longitude)&&latitude>=-90&&latitude<=90&&longitude>=-180&&longitude<=180;
    public static string Condition(int? code)=>code switch {
        0=>"Clear sky",1=>"Mainly clear",2=>"Partly cloudy",3=>"Overcast",45 or 48=>"Foggy",
        51 or 53 or 55=>"Drizzle",56 or 57=>"Freezing drizzle",61=>"Light rain",63=>"Rain",65=>"Heavy rain",66 or 67=>"Freezing rain",
        71 or 73 or 75 or 77=>"Snow",80 or 81 or 82=>"Rain showers",85 or 86=>"Snow showers",95 or 96 or 97 or 99=>"Thunderstorms",_=>"Conditions unavailable"};

    static double? OptionalNumber(JsonElement value,string name,double min,double max)
    {
        if(!value.TryGetProperty(name,out var item)||item.ValueKind==JsonValueKind.Null)return null;
        if(!item.TryGetDouble(out var number)||!double.IsFinite(number)||number<min||number>max)throw new InvalidDataException();
        return number;
    }
    static int? Code(JsonElement value,string name)
    {
        var number=OptionalNumber(value,name,0,999);
        if(number is not null&&number!=Math.Truncate(number.Value))throw new InvalidDataException();
        return number is null?null:(int)number.Value;
    }
    static string Text(JsonElement value,string name,int max=150) => value.TryGetProperty(name,out var item)&&item.ValueKind==JsonValueKind.String?(item.GetString()??"")[..Math.Min((item.GetString()??"").Length,max)]:"";
    static List<WeatherDay> Forecast(JsonElement root)
    {
        if(!root.TryGetProperty("daily",out var daily))return [];
        var dates=daily.GetProperty("time");var highs=daily.GetProperty("temperature_2m_max");var lows=daily.GetProperty("temperature_2m_min");
        if(dates.ValueKind!=JsonValueKind.Array||highs.ValueKind!=JsonValueKind.Array||lows.ValueKind!=JsonValueKind.Array||dates.GetArrayLength()!=highs.GetArrayLength()||dates.GetArrayLength()!=lows.GetArrayLength())throw new InvalidDataException();
        var hasCodes=daily.TryGetProperty("weather_code",out var codes);var hasRain=daily.TryGetProperty("precipitation_probability_max",out var rain);
        if(hasCodes&&(codes.ValueKind!=JsonValueKind.Array||codes.GetArrayLength()!=dates.GetArrayLength()))throw new InvalidDataException();
        if(hasRain&&(rain.ValueKind!=JsonValueKind.Array||rain.GetArrayLength()!=dates.GetArrayLength()))throw new InvalidDataException();
        var result=new List<WeatherDay>();
        for(var i=0;i<Math.Min(3,dates.GetArrayLength());i++)
        {
            var date=dates[i].GetString();var high=highs[i].GetDouble();var low=lows[i].GetDouble();
            if(!DateOnly.TryParseExact(date,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out _)||!ValidTemp(high)||!ValidTemp(low)||low>high)throw new InvalidDataException();
            int? probability=hasRain&&rain[i].ValueKind!=JsonValueKind.Null?rain[i].GetInt32():null;
            if(probability is <0 or >100)throw new InvalidDataException();
            var code=hasCodes&&codes[i].ValueKind!=JsonValueKind.Null?codes[i].GetInt32():(int?)null;
            if(code is <0 or >999)throw new InvalidDataException();
            result.Add(new WeatherDay(date!,high,low,probability,Condition(code)));
        }
        return result;
    }
    static bool UpstreamFailure(Exception ex)=>ex is HttpRequestException or OperationCanceledException or JsonException or InvalidDataException or KeyNotFoundException or InvalidOperationException or FormatException or OverflowException;

    public async Task<object> Get(CancellationToken ct,double latitude=13.62,double longitude=123.19)
    {
        if(!ValidCoordinates(latitude,longitude))throw new ArgumentOutOfRangeException(nameof(latitude));
        latitude=Math.Round(latitude,2);longitude=Math.Round(longitude,2);
        try {
            var url=FormattableString.Invariant($"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&current=temperature_2m,relative_humidity_2m,apparent_temperature,weather_code,wind_speed_10m&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max&forecast_days=3&timezone=auto");
            using var response=await client.GetAsync(url,ct);response.EnsureSuccessStatusCode();
            using var document=JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));var root=document.RootElement;var current=root.GetProperty("current");
            var temperature=current.GetProperty("temperature_2m").GetDouble();if(!ValidTemp(temperature))throw new InvalidDataException();
            var humidity=OptionalNumber(current,"relative_humidity_2m",0,100);var wind=OptionalNumber(current,"wind_speed_10m",0,500);var feelsLike=OptionalNumber(current,"apparent_temperature",-100,70);
            return new{available=true,temperatureC=temperature,humidityPercent=humidity,windKmh=wind,feelsLikeC=feelsLike,condition=Condition(Code(current,"weather_code")),observedAt=Text(current,"time",40),timezone=Text(root,"timezone",80),checkedAt=DateTimeOffset.UtcNow,forecast=Forecast(root),source="Open-Meteo"};
        }catch(Exception ex)when(UpstreamFailure(ex)){return new{available=false,message="Weather is temporarily unavailable. Please try again shortly.",source="Open-Meteo"};}
    }

    public async Task<object> Search(string query,CancellationToken ct)
    {
        try {
            var url="https://geocoding-api.open-meteo.com/v1/search?name="+Uri.EscapeDataString(query.Trim())+"&count=5&language=en&format=json";
            using var response=await client.GetAsync(url,ct);response.EnsureSuccessStatusCode();
            using var document=JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));var places=new List<WeatherPlace>();
            if(document.RootElement.TryGetProperty("results",out var results)){
                if(results.ValueKind!=JsonValueKind.Array)throw new InvalidDataException();
                foreach(var result in results.EnumerateArray().Take(5)){
                    var name=Text(result,"name");var latitude=result.GetProperty("latitude").GetDouble();var longitude=result.GetProperty("longitude").GetDouble();
                    if(string.IsNullOrWhiteSpace(name)||!ValidCoordinates(latitude,longitude))throw new InvalidDataException();
                    places.Add(new WeatherPlace(name,Text(result,"admin1"),Text(result,"country"),latitude,longitude));
                }
            }
            return new{available=true,locations=places};
        }catch(Exception ex)when(UpstreamFailure(ex)){return new{available=false,locations=Array.Empty<WeatherPlace>(),message="Location search is temporarily unavailable. Please try again shortly."};}
    }
}
