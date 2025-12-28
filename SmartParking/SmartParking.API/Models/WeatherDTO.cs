// -----------------------------------------------------------------------------
// Projeto: SmartParking
// Unidade Curricular: ISI (IPCA)
// Autor: Diogo Graça
// Ficheiro: WeatherDTO.cs
// Descrição: DTOs para desserialização de resposta de API externa de meteorologia.
// Notas:
//  - JsonPropertyName mapeia os nomes do JSON externo para as propriedades C#.
// -----------------------------------------------------------------------------

using System.Text.Json.Serialization;

namespace SmartParking.API.Models
{
    #region DTO: WeatherResponse (API Externa)

    /// <summary>
    /// Estrutura principal da resposta do serviço externo de meteorologia.
    /// Contém blocos relevantes como "main" e "weather".
    /// </summary>
    public class WeatherResponse
    {
        #region Propriedades

        /// <summary>
        /// Bloco "main" da API externa, normalmente com temperatura e métricas agregadas.
        /// </summary>
        [JsonPropertyName("main")]
        public MainData Main { get; set; }

        /// <summary>
        /// Array "weather" com descrições e estados (ex.: "clear sky", "rain").
        /// </summary>
        [JsonPropertyName("weather")]
        public WeatherDescription[] Weather { get; set; }

        #endregion
    }

    /// <summary>
    /// DTO para o bloco "main" da resposta (ex.: temperatura).
    /// </summary>
    public class MainData
    {
        #region Propriedades

        /// <summary>
        /// Temperatura devolvida pela API (dependente de units=metric para Celsius).
        /// </summary>
        [JsonPropertyName("temp")]
        public float Temp { get; set; }

        #endregion
    }

    /// <summary>
    /// DTO para cada item do array "weather".
    /// </summary>
    public class WeatherDescription
    {
        #region Propriedades

        /// <summary>
        /// Descrição textual do estado meteorológico.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        #endregion
    }

    #endregion
}