using System.Text.Json.Serialization;

namespace SmartParking.API.Models
{
    public class WeatherResponse
    {
        [JsonPropertyName("main")]
        public MainData Main { get; set; }

        [JsonPropertyName("weather")]
        public WeatherDescription[] Weather { get; set; }
    }

    public class MainData
    {
        [JsonPropertyName("temp")]
        public float Temp { get; set; }
    }

    public class WeatherDescription
    {
        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}