// -----------------------------------------------------------------------------
// Projeto: SmartParking
// Unidade Curricular: ISI (IPCA)
// Autor: Diogo Graça
// Ficheiro: AuthController.cs
// Descrição: Endpoints de autenticação (Registo e Login) com emissão de JWT.
// Notas:
//  - Este controlador devolve também a Role para controlo de UI no frontend.
//  - Em contexto real, Password nunca deve ser guardada em texto simples.
// -----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartParking.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartParking.API.Controllers
{
    /// <summary>
    /// Controller responsável pela autenticação de utilizadores:
    /// - Registo (cria utilizadores com Role="User")
    /// - Login (valida credenciais e devolve JWT + Role)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        #region Campos privados

        /// <summary>
        /// Contexto EF Core para acesso ao repositório de utilizadores.
        /// </summary>
        private readonly ParkingContext _context;

        #endregion

        #region Construtor

        /// <summary>
        /// Construtor com injeção de dependência do contexto.
        /// </summary>
        /// <param name="context">ParkingContext (EF Core).</param>
        public AuthController(ParkingContext context)
        {
            _context = context;
        }

        #endregion

        #region Endpoints - Autenticação

        /// <summary>
        /// Registo de um novo utilizador.
        /// Por desenho, a Role é sempre atribuída como "User".
        /// </summary>
        /// <param name="request">Credenciais de registo (Username/Password).</param>
        /// <returns>200 OK se criado; 400 BadRequest se já existir.</returns>
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

        /// <summary>
        /// Login do utilizador e emissão de token JWT.
        /// O token inclui:
        /// - Claim "id"
        /// - Claim de Role (ClaimTypes.Role)
        /// </summary>
        /// <param name="request">Credenciais (Username/Password).</param>
        /// <returns>200 OK com Token e Role; 401 Unauthorized se falhar.</returns>
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

        #endregion
    }

    #region DTOs

    /// <summary>
    /// DTO de autenticação utilizado para Registo e Login.
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Nome de utilizador.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Palavra-passe (em contexto real deve ser hash + salt).
        /// </summary>
        public string Password { get; set; }
    }

    #endregion
}