using System.Text.Json;
using SmartParking.API.Models;

namespace SmartParking.API.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;
        // Já pus a tua chave aqui:
        private readonly string _apiKey = "1c61f7257faf7285ff220f8abdfae717"; 

        public WeatherService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetTempoInfo(string cidade)
        {
            // Se a cidade vier vazia, usamos Barcelos por defeito
            if (string.IsNullOrWhiteSpace(cidade)) cidade = "Barcelos";

            try
            {
                string url = $"https://api.openweathermap.org/data/2.5/weather?q={cidade}&appid={_apiKey}&units=metric&lang=pt";
                
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode) 
                {
                    // Isto vai mostrar o erro real no Swagger (ex: "401 Unauthorized")
                    return $"Erro Externo: {response.StatusCode}"; 
                }

                var json = await response.Content.ReadAsStringAsync();
                var dados = JsonSerializer.Deserialize<WeatherResponse>(json);

                if (dados == null) return "Erro leitura";

                return $"{dados.Main.Temp:F1}ºC, {dados.Weather[0].Description}";
            }
            catch
            {
                return "Serviço indisponível";
            }
        }
    }
}