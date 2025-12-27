namespace SmartParking.API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // Em produção, isto seria um Hash!
        public string Role { get; set; } = "User";
    }
}