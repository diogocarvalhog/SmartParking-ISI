// -----------------------------------------------------------------------------
// Projeto: SmartParking
// Unidade Curricular: ISI (IPCA)
// Autor: Diogo Graça
// Ficheiro: ParquesController.cs
// -----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartParking.API.Models;
using SmartParking.API.Services;

namespace SmartParking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParquesController : ControllerBase
    {
        private readonly ParkingContext _context;

        public ParquesController(ParkingContext context)
        {
            _context = context;
        }

        #region Endpoints REST - CRUD

        // 1. LISTAR TODOS
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Parque>>> GetParques()
        {
            return await _context.Parques.ToListAsync();
        }

        // 2. OBTER DETALHE (O que faltava para o teste 5 do Postman)
        // GET: api/Parques/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Parque>> GetParque(int id)
        {
            var parque = await _context.Parques.FindAsync(id);

            if (parque == null) return NotFound("Parque não encontrado.");

            return Ok(parque);
        }

        // 3. CRIAR (POST)
        [HttpPost]
        public async Task<ActionResult<Parque>> PostParque(Parque parque)
        {
            _context.Parques.Add(parque);
            await _context.SaveChangesAsync();
            
            // Importante: devolve o objeto criado para o Postman ler o ID
            return CreatedAtAction(nameof(GetParque), new { id = parque.Id }, parque);
        }

        // 4. REMOVER (DELETE)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteParque(int id)
        {
            var parque = await _context.Parques
                .Include(p => p.Lugares)
                .ThenInclude(l => l.Sensor)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (parque == null) return NotFound("Parque não encontrado.");

            foreach (var lugar in parque.Lugares)
            {
                if (lugar.Sensor != null) _context.Sensores.Remove(lugar.Sensor);
                _context.Lugares.Remove(lugar);
            }

            _context.Parques.Remove(parque);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        #endregion

        #region Integração Meteorológica

        [HttpGet("{id}/weather")]
        public async Task<IActionResult> GetParkWeather(int id, [FromServices] WeatherService weatherService)
        {
            var parque = await _context.Parques.FindAsync(id);
            if (parque == null) return NotFound("Parque não encontrado");

            if (!parque.IsExterior)
            {
                return Ok(new WeatherDto { Temp = 22, Description = "Interior (Climatizado)", Icon = "indoor", City = parque.Localizacao });
            }

            var weather = await weatherService.GetWeatherAsync(parque.Latitude, parque.Longitude);
            return weather == null ? BadRequest("Erro na API externa.") : Ok(weather);
        }

        #endregion
    }
}