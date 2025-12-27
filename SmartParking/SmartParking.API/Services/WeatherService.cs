using System.Text.Json;

namespace SmartParking.API.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;
        // A tua chave real:
        private const string API_KEY = "1c61f7257faf7285ff220f8abdfae717"; 

        public WeatherService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<WeatherDto?> GetWeatherAsync(decimal lat, decimal lon)
        {
            // URL da OpenWeatherMap (units=metric para Celsius, lang=pt para Português)
            var url = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={API_KEY}&units=metric&lang=pt";

            try 
            {
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;

                var json = await response.Content.ReadAsStringAsync();
                
                // Deserializar o JSON complexo da API para o nosso objeto simples
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var weatherData = JsonSerializer.Deserialize<OpenWeatherResponse>(json, options);

                return new WeatherDto
                {
                    Temp = weatherData?.Main?.Temp ?? 0,
                    Description = weatherData?.Weather?.FirstOrDefault()?.Description ?? "Desconhecido",
                    Icon = weatherData?.Weather?.FirstOrDefault()?.Icon ?? "01d",
                    City = weatherData?.Name
                };
            }
            catch 
            {
                return null; 
            }
        }
    }

    // Classes auxiliares para ler o JSON da OpenWeather
    public class WeatherDto 
    {
        public double Temp { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string? City { get; set; }
    }

    public class OpenWeatherResponse
    {
        public MainData Main { get; set; }
        public List<WeatherInfo> Weather { get; set; }
        public string Name { get; set; }
    }
    public class MainData { public double Temp { get; set; } }
    public class WeatherInfo { public string Description { get; set; } public string Icon { get; set; } }
}