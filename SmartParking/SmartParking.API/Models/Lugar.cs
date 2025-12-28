// -----------------------------------------------------------------------------
// Projeto: SmartParking
// Unidade Curricular: ISI (IPCA)
// Autor: Diogo Graça
// Ficheiro: Lugar.cs
// Descrição: Entidade EF Core que representa um Lugar de estacionamento.
// Notas:
//  - A navegação para Parque é ignorada em JSON para evitar ciclos na serialização REST.
//  - A navegação para Sensor permite monitorização do estado de ocupação.
// -----------------------------------------------------------------------------

﻿using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SmartParking.API.Models;

#region Entidade: Lugar

/// <summary>
/// Representa um lugar de estacionamento dentro de um parque.
/// </summary>
public partial class Lugar
{
    #region Propriedades (Persistência)

    /// <summary>
    /// Identificador único do lugar.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Número/identificação do lugar (ex.: "A01", "12", "B-3").
    /// </summary>
    public string NumeroLugar { get; set; } = null!;

    /// <summary>
    /// Piso onde o lugar está localizado.
    /// </summary>
    public int Piso { get; set; }

    /// <summary>
    /// Chave estrangeira para o parque ao qual o lugar pertence.
    /// </summary>
    public int ParqueId { get; set; }

    #endregion

    #region Navegação (Relações)

    /// <summary>
    /// Referência ao parque (navegação).
    /// Ignorada em JSON para evitar ciclos (Parque -> Lugares -> Parque).
    /// </summary>
    [JsonIgnore]
    public virtual Parque? Parque { get; set; } = null!;
    
    /// <summary>
    /// Sensor associado ao lugar (relação 1:1, opcional).
    /// </summary>
    public virtual Sensor? Sensor { get; set; }

    #endregion
}

#endregion