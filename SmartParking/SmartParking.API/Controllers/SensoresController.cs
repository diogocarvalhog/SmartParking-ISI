using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartParking.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace SmartParking.API.Controllers
{   
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SensoresController : ControllerBase
    {
        private readonly ParkingContext _context;

        public SensoresController(ParkingContext context)
        {
            _context = context;
        }

        // GET: api/Sensores
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sensor>>> GetSensores()
        {
            return await _context.Sensores.ToListAsync();
        }

        // POST: api/Sensores
        [HttpPost]
        public async Task<ActionResult<Sensor>> PostSensor(Sensor sensor)
        {
            var lugar = await _context.Lugares.FindAsync(sensor.LugarId);
            if (lugar == null) return BadRequest("O LugarID especificado não existe.");

            var existe = await _context.Sensores.AnyAsync(s => s.LugarId == sensor.LugarId);
            if (existe) return BadRequest("Este lugar já tem um sensor instalado.");

            sensor.UltimaAtualizacao = DateTime.UtcNow;
            _context.Sensores.Add(sensor);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSensores", new { id = sensor.Id }, sensor);
        }

        // PUT: api/Sensores/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSensor(int id, Sensor sensorAtualizado)
        {
            var sensor = await _context.Sensores.FindAsync(id);
            if (sensor == null) return NotFound();

            sensor.Estado = sensorAtualizado.Estado;
            sensor.UltimaAtualizacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- NOVO: O INTERRUPTOR PARA O REACT ---
        // POST: api/Sensores/5/toggle
        [HttpPost("{id}/toggle")]
        public async Task<IActionResult> ToggleSensor(int id)
        {
            var sensor = await _context.Sensores.FindAsync(id);
            if (sensor == null) return NotFound("Sensor não encontrado.");

            // Inverte o estado (true virou false, false virou true)
            sensor.Estado = !sensor.Estado;
            sensor.UltimaAtualizacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { 
                Message = $"Estado alterado para {(sensor.Estado ? "Ocupado" : "Livre")}", 
                NovoEstado = sensor.Estado 
            });
        }

        // DELETE: api/Sensores/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSensor(int id)
        {
            var sensor = await _context.Sensores.FindAsync(id);
            if (sensor == null) return NotFound();

            _context.Sensores.Remove(sensor);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // GET: api/Sensores/Avariados
        [HttpGet("Avariados")]
        public async Task<ActionResult<IEnumerable<Sensor>>> GetSensoresAvariados()
        {
            var limite = DateTime.UtcNow.AddHours(-24);
            return await _context.Sensores
                .Where(s => s.UltimaAtualizacao < limite)
                .ToListAsync();
        }
    }
}