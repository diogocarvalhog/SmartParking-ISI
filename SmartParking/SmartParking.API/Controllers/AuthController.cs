using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartParking.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartParking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ParkingContext _context;

        public AuthController(ParkingContext context)
        {
            _context = context;
        }

        // REGISTAR: Cria sempre como "User"
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] LoginRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest("Utilizador já existe!");

            var novoUser = new User
            {
                Username = request.Username,
                Password = request.Password,
                Role = "User" // <--- Padrão seguro. Ninguém vira Admin aqui.
            };

            _context.Users.Add(novoUser);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Conta criada com sucesso!" });
        }

        // LOGIN: Devolve o Token e a Role
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == request.Password);

            if (user == null) return Unauthorized("Dados incorretos.");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(SmartParking.API.Settings.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { 
                    new Claim("id", user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role) 
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // Devolvemos a Role para o React saber se mostra o botão Admin
            return Ok(new { Token = tokenString, Role = user.Role });
        }
    }

    public class LoginRequest {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}