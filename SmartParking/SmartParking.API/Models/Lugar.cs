using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SmartParking.API.Models;

public partial class Lugar
{
    public int Id { get; set; }

    public string NumeroLugar { get; set; } = null!;

    public int Piso { get; set; }

    public int ParqueId { get; set; }

    [JsonIgnore]
    public virtual Parque? Parque { get; set; } = null!;
    
    public virtual Sensor? Sensor { get; set; }
}
