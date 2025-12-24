using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SmartParking.API.Models;

public partial class Sensor
{
    public int Id { get; set; }

    public string? Tipo { get; set; }

    public bool Estado { get; set; }

    public DateTime? UltimaAtualizacao { get; set; }

    public int LugarId { get; set; }

    [JsonIgnore]
    public virtual Lugar? Lugar { get; set; } = null!;
}
