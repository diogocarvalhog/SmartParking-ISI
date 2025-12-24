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

        // POST: api/Sensores (Instalar novo sensor num lugar)
        [HttpPost]
        public async Task<ActionResult<Sensor>> PostSensor(Sensor sensor)
        {
            // Validação: O lugar existe?
            var lugar = await _context.Lugares.FindAsync(sensor.LugarId);
            if (lugar == null)
            {
                return BadRequest("O LugarID especificado não existe.");
            }

            // Validação: O lugar já tem sensor?
            var existe = await _context.Sensores.AnyAsync(s => s.LugarId == sensor.LugarId);
            if (existe)
            {
                return BadRequest("Este lugar já tem um sensor instalado.");
            }

            sensor.UltimaAtualizacao = DateTime.UtcNow;
            _context.Sensores.Add(sensor);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSensores", new { id = sensor.Id }, sensor);
        }

        // PUT: api/Sensores/5 (Atualizar valor - Simulação do Hardware)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSensor(int id, Sensor sensorAtualizado)
        {
            var sensor = await _context.Sensores.FindAsync(id);
            if (sensor == null) return NotFound();

            // Atualiza apenas o estado e a data
            sensor.Estado = sensorAtualizado.Estado;
            sensor.UltimaAtualizacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Sensores/5 (Remover sensor avariado)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSensor(int id)
        {
            var sensor = await _context.Sensores.FindAsync(id);
            if (sensor == null) return NotFound();

            _context.Sensores.Remove(sensor);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- FUNCIONALIDADE EXTRA (O que pediste) ---
        
        // GET: api/Sensores/Avariados
        // Lista sensores que não dão sinal há mais de 24 horas
        [HttpGet("Avariados")]
        public async Task<ActionResult<IEnumerable<Sensor>>> GetSensoresAvariados()
        {
            // Define o limite (ex: ontem à mesma hora)
            var limite = DateTime.UtcNow.AddHours(-24);

            return await _context.Sensores
                .Where(s => s.UltimaAtualizacao < limite)
                .ToListAsync();
        }
    }
}