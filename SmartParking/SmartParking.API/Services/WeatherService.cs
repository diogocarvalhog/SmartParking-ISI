//-----------------------------------------------------------------
// <copyright file="WeatherService.cs" company="IPCA">
// Copyright IPCA-EST. All rights reserved.
// </copyright>
// <date>28-12-2025</date>
// <version>1.1</version>
// <author>Diogo Graça</author>
// <description>Serviço de integração com API OpenWeather (REST)</description>
//-----------------------------------------------------------------

using System.Text.Json;
using System.Globalization; // Necessário para formatar números com ponto

namespace SmartParking.API.Services
{
    /// <summary>
    /// Classe responsável pela interoperabilidade com o serviço externo OpenWeather.
    /// </summary>
    public class WeatherService
    {
        #region Atributos e Constantes
        private readonly HttpClient _httpClient;
        private const string API_KEY = "1c61f7257faf7285ff220f8abdfae717"; 
        #endregion

        #region Construtor
        public WeatherService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        #endregion

        #region Métodos de Integração
        /// <summary>
        /// Obtém a meteorologia atual via API REST externa.
        /// </summary>
        public async Task<WeatherDto?> GetWeatherAsync(decimal lat, decimal lon)
        {
            // SOLUÇÃO: Forçar CultureInfo.InvariantCulture garante que lat/lon usam ponto '.' e não vírgula ','
            string sLat = lat.ToString(CultureInfo.InvariantCulture);
            string sLon = lon.ToString(CultureInfo.InvariantCulture);

            var url = $"https://api.openweathermap.org/data/2.5/weather?lat={sLat}&lon={sLon}&appid={API_KEY}&units=metric&lang=pt";

            try 
            {
                var response = await _httpClient.GetAsync(url);
                
                // Se a API externa falhar, não crashamos o sistema, devolvemos null
                if (!response.IsSuccessStatusCode) return null;

                var json = await response.Content.ReadAsStringAsync();
                
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var weatherData = JsonSerializer.Deserialize<OpenWeatherResponse>(json, options);

                return new WeatherDto
                {
                    Temp = weatherData?.Main?.Temp ?? 0,
                    Description = weatherData?.Weather?.FirstOrDefault()?.Description ?? "Sem dados",
                    Icon = weatherData?.Weather?.FirstOrDefault()?.Icon ?? "01d",
                    City = weatherData?.Name
                };
            }
            catch (Exception)
            {
                return null; 
            }
        }
        #endregion
    }

    #region Modelos de Dados (DTOs e Respostas)
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
    #endregion
}