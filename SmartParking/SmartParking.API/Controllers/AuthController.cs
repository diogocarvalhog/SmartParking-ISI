using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace SmartParking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        // POST: api/Auth/Login
        //Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjEiLCJyb2xlIjoiQWRtaW4iLCJuYmYiOjE3NjY1OTg5MjgsImV4cCI6MTc2NjYwNjEyOCwiaWF0IjoxNzY2NTk4OTI4fQ.Ktb8MGLl3lELHcdzFMg5rT-X0lhBE9AvwOSTS89q31E
        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // 1. Simulação de validação (No mundo real, irias verificar na BD de Utilizadores)
            if (request.Username == "admin" && request.Password == "admin123")
            {
                // 2. Criar o Token (O Crachá)
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes("CHAVE_SUPER_SECRETA_DO_DIOGO_PARKING_2025"); // Tem de ser igual à do Program.cs!
                
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[] { new Claim("id", "1"), new Claim("role", "Admin") }),
                    Expires = DateTime.UtcNow.AddHours(2), // O cartão expira em 2 horas
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                return Ok(new { Token = tokenString });
            }

            return Unauthorized("Username ou Password errados!");
        }
    }

    // Classe simples para receber os dados
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}