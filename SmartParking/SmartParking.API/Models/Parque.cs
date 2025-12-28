// -----------------------------------------------------------------------------
// Projeto: SmartParking
// Unidade Curricular: ISI (IPCA)
// Autor: Diogo Graça
// Ficheiro: Parque.cs
// Descrição: Entidade EF Core que representa um Parque de estacionamento.
// Notas:
//  - Latitude/Longitude suportam integração com serviço externo (meteorologia).
//  - Lugares é ignorado em JSON para evitar ciclos em respostas REST.
// -----------------------------------------------------------------------------

﻿namespace SmartParking.API.Models
{
    #region Entidade: Parque

    /// <summary>
    /// Representa um parque de estacionamento.
    /// </summary>
    public class Parque
    {
        #region Propriedades (Persistência)

        /// <summary>
        /// Identificador único do parque.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome do parque.
        /// </summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Localização textual do parque (ex.: cidade, zona, morada resumida).
        /// </summary>
        public string? Localizacao { get; set; }

        /// <summary>
        /// Capacidade total (número máximo de lugares).
        /// </summary>
        public int CapacidadeTotal { get; set; }
        
        /// <summary>
        /// Latitude usada para chamadas a serviços externos (ex.: OpenWeather).
        /// </summary>
        public decimal Latitude { get; set; }

        /// <summary>
        /// Longitude usada para chamadas a serviços externos (ex.: OpenWeather).
        /// </summary>
        public decimal Longitude { get; set; }
        
        /// <summary>
        /// Indica se o parque é exterior (true) ou interior (false).
        /// </summary>
        public bool IsExterior { get; set; } = true;

        #endregion

        #region Navegação (Relações)

        /// <summary>
        /// Coleção de lugares associados ao parque (relação 1:N).
        /// Ignorada em JSON para evitar ciclos na serialização.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public List<Lugar> Lugares { get; set; } = new List<Lugar>();

        #endregion
    }

    #endregion
}