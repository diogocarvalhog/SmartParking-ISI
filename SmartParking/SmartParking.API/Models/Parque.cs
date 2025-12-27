namespace SmartParking.API.Models
{
    public class Parque
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Localizacao { get; set; }
        public int CapacidadeTotal { get; set; }
        
        // Para a API do Tempo funcionar:
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        
        // Define se chove nos carros (True) ou se é garagem fechada (False)
        public bool IsExterior { get; set; } = true;

        // Relação com Lugares
        [System.Text.Json.Serialization.JsonIgnore]
        public List<Lugar> Lugares { get; set; } = new List<Lugar>();
    }
}