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

        // GET: api/Parques
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Parque>>> GetParques()
        {
            return await _context.Parques.ToListAsync();
        }

        // POST: api/Parques
        [HttpPost]
        public async Task<ActionResult<Parque>> PostParque(Parque parque)
        {
            _context.Parques.Add(parque);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetParques", new { id = parque.Id }, parque);
        }

        // GET: api/Parques/5/weather
        [HttpGet("{id}/weather")]
        public async Task<IActionResult> GetParkWeather(int id, [FromServices] WeatherService weatherService)
        {
            var parque = await _context.Parques.FindAsync(id);
            if (parque == null) return NotFound("Parque não encontrado");

            if (!parque.IsExterior) 
            {
                return Ok(new WeatherDto { 
                    Temp = 22, 
                    Description = "Interior (Climatizado)", 
                    Icon = "indoor",
                    City = parque.Localizacao
                });
            }

            var weather = await weatherService.GetWeatherAsync(parque.Latitude, parque.Longitude);
            if (weather == null) return BadRequest("Não foi possível obter a meteorologia.");
            return Ok(weather);
        }

        // --- NOVO MÉTODO: DELETE api/Parques/5 ---
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteParque(int id)
        {
            var parque = await _context.Parques.FindAsync(id);
            if (parque == null)
            {
                return NotFound();
            }

            _context.Parques.Remove(parque);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}