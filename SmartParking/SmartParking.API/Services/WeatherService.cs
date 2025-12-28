// -----------------------------------------------------------------------------
// Projeto: SmartParking
// Unidade Curricular: ISI (IPCA)
// Autor: Diogo Graça
// Ficheiro: WeatherService.cs
// Descrição: Serviço de integração com API externa de meteorologia (OpenWeather).
// Notas:
//  - Usa HttpClient (DI) para chamadas HTTP.
//  - Desserializa resposta JSON para classes locais (OpenWeatherResponse).
//  - Constrói um DTO simplificado (WeatherDto) consumível pelos controllers.
// -----------------------------------------------------------------------------

using System.Text.Json;

namespace SmartParking.API.Services
{
    #region Serviço Externo: WeatherService

    /// <summary>
    /// Serviço de integração com API externa de meteorologia.
    /// </summary>
    public class WeatherService
    {
        #region Campos privados

        /// <summary>
        /// Cliente HTTP injetado por DI para chamadas à API externa.
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Chave de API usada na autenticação com o fornecedor externo.
        /// </summary>
        private const string API_KEY = "1c61f7257faf7285ff220f8abdfae717"; 

        #endregion

        #region Construtor

        /// <summary>
        /// Construtor com injeção do HttpClient.
        /// </summary>
        /// <param name="httpClient">HttpClient fornecido pelo DI.</param>
        public WeatherService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        #endregion

        #region Métodos públicos

        /// <summary>
        /// Obtém informação meteorológica para as coordenadas fornecidas.
        /// </summary>
        /// <param name="lat">Latitude.</param>
        /// <param name="lon">Longitude.</param>
        /// <returns>WeatherDto com dados simplificados, ou null em caso de falha.</returns>
        public async Task<WeatherDto?> GetWeatherAsync(decimal lat, decimal lon)
        {
            // URL da OpenWeatherMap (units=metric para Celsius, lang=pt para Português)
            var url = $"https://api.openweathermap.org/data/...ather?lat={lat}&lon={lon}&appid={API_KEY}&units=metric&lang=pt";

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

        #endregion
    }

    #endregion

    #region DTO Simplificado: WeatherDto

    /// <summary>
    /// DTO simplificado consumido pelo sistema (frontend/controllers),
    /// derivado de uma resposta externa mais complexa.
    /// </summary>
    public class WeatherDto
    {
        /// <summary>
        /// Temperatura (Celsius quando units=metric).
        /// </summary>
        public double Temp { get; set; }

        /// <summary>
        /// Descrição textual do estado do tempo.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Ícone/código devolvido pelo fornecedor externo (ex.: "01d").
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Nome da cidade/localidade (quando disponível).
        /// </summary>
        public string? City { get; set; }
    }

    #endregion

    #region Modelos de Desserialização: OpenWeatherResponse

    /// <summary>
    /// Estrutura de desserialização da resposta do fornecedor externo.
    /// Modela apenas os campos usados no projeto.
    /// </summary>
    public class OpenWeatherResponse
    {
        public MainData Main { get; set; }
        public List<WeatherInfo> Weather { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// Bloco "main" do fornecedor externo.
    /// </summary>
    public class MainData { public double Temp { get; set; } }

    /// <summary>
    /// Item do array "weather" (descrição e ícone).
    /// </summary>
    public class WeatherInfo { public string Description { get; set; } public string Icon { get; set; } }

    #endregion
}