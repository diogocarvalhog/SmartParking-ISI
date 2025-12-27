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

        // GET: api/Parques (Lista todos os parques)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Parque>>> GetParques()
        {
            return await _context.Parques.ToListAsync();
        }

        // POST: api/Parques (Criar novo parque - Para o Admin)
        [HttpPost]
        public async Task<ActionResult<Parque>> PostParque(Parque parque)
        {
            _context.Parques.Add(parque);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetParques", new { id = parque.Id }, parque);
        }

        // GET: api/Parques/5/weather (Saber o tempo neste parque)
        [HttpGet("{id}/weather")]
        public async Task<IActionResult> GetParkWeather(int id, [FromServices] WeatherService weatherService)
        {
            var parque = await _context.Parques.FindAsync(id);
            if (parque == null) return NotFound("Parque não encontrado");

            // Se for interior, não precisamos de ir à API
            if (!parque.IsExterior) 
            {
                return Ok(new WeatherDto { 
                    Temp = 22, 
                    Description = "Interior (Climatizado)", 
                    Icon = "indoor",
                    City = parque.Localizacao
                });
            }

            // Busca o tempo real usando a tua API Key
            var weather = await weatherService.GetWeatherAsync(parque.Latitude, parque.Longitude);
            
            if (weather == null) return BadRequest("Não foi possível obter a meteorologia.");
            
            return Ok(weather);
        }
    }
}