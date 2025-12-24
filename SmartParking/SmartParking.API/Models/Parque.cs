using System;
using System.Collections.Generic;

namespace SmartParking.API.Models;

public partial class Parque
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public string? Localizacao { get; set; }

    public int CapacidadeTotal { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public virtual ICollection<Lugar> Lugares { get; set; } = new List<Lugar>();
}
