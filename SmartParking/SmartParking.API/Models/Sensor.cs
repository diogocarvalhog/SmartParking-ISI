// -----------------------------------------------------------------------------
// Projeto: SmartParking
// Unidade Curricular: ISI (IPCA)
// Autor: Diogo Graça
// Ficheiro: Sensor.cs
// Descrição: Entidade EF Core que representa um Sensor associado a um Lugar.
// Notas:
//  - Lugar é ignorado em JSON para evitar ciclos em respostas REST.
//  - UltimaAtualizacao permite inferir inatividade/avaria e calcular tempos.
// -----------------------------------------------------------------------------

﻿using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SmartParking.API.Models;

#region Entidade: Sensor

/// <summary>
/// Representa um sensor (por exemplo, de presença) instalado num lugar.
/// </summary>
public partial class Sensor
{
    #region Propriedades (Persistência)

    /// <summary>
    /// Identificador único do sensor.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Tipo do sensor (ex.: "Presenca").
    /// </summary>
    public string? Tipo { get; set; }

    /// <summary>
    /// Estado do sensor:
    /// - true: ocupado
    /// - false: livre
    /// </summary>
    public bool Estado { get; set; }

    /// <summary>
    /// Data/hora da última atualização do sensor.
    /// </summary>
    public DateTime? UltimaAtualizacao { get; set; }

    /// <summary>
    /// Chave estrangeira para o lugar associado (relação 1:1).
    /// </summary>
    public int LugarId { get; set; }

    #endregion

    #region Navegação (Relações)

    /// <summary>
    /// Navegação para o lugar associado.
    /// Ignorada em JSON para evitar ciclos (Lugar -> Sensor -> Lugar).
    /// </summary>
    [JsonIgnore]
    public virtual Lugar? Lugar { get; set; } = null!;

    #endregion
}

#endregion