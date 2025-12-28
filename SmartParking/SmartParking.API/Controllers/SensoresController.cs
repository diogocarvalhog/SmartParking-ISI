// -----------------------------------------------------------------------------
// Projeto: SmartParking
// Unidade Curricular: ISI (IPCA)
// Autor: Diogo Graça
// Ficheiro: SensoresController.cs
// Descrição: Endpoints REST para gestão de sensores, incluindo:
//  - instalação/validação (1 sensor por lugar)
//  - update de estado
//  - toggle para integração direta com o frontend
//  - consulta de sensores "avariados" por timeout de atualização
// -----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartParking.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace SmartParking.API.Controllers
{   
    /// <summary>
    /// Controller REST para gestão de sensores.
    /// Protegido por autenticação (JWT) via [Authorize].
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SensoresController : ControllerBase
    {
        #region Campos privados

        /// <summary>
        /// Contexto EF Core para acesso a Sensores e validação com Lugares.
        /// </summary>
        private readonly ParkingContext _context;

        #endregion

        #region Construtor

        /// <summary>
        /// Construtor com injeção do contexto EF.
        /// </summary>
        /// <param name="context">ParkingContext (EF Core).</param>
        public SensoresController(ParkingContext context)
        {
            _context = context;
        }

        #endregion

        #region Endpoints REST - CRUD

        /// <summary>
        /// Lista todos os sensores.
        /// </summary>
        /// <returns>Lista de sensores.</returns>
        // GET: api/Sensores
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sensor>>> GetSensores()
        {
            return await _context.Sensores.ToListAsync();
        }

        /// <summary>
        /// Cria/instala um sensor num lugar.
        /// Regras:
        /// - O LugarID deve existir
        /// - Só pode existir 1 sensor por lugar (validação de unicidade)
        /// </summary>
        /// <param name="sensor">Sensor a inserir (inclui LugarId).</param>
        /// <returns>201 Created; 400 BadRequest; 404/validação conforme regras.</returns>
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

        /// <summary>
        /// Atualiza estado de um sensor existente.
        /// Atualiza também UltimaAtualizacao para refletir evento de alteração.
        /// </summary>
        /// <param name="id">ID do sensor.</param>
        /// <param name="sensorAtualizado">Dados com novo estado (Estado).</param>
        /// <returns>204 NoContent; 404 NotFound.</returns>
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

        /// <summary>
        /// Endpoint simplificado para UI (toggle on/off).
        /// Inverte o estado atual e atualiza timestamp.
        /// </summary>
        /// <param name="id">ID do sensor.</param>
        /// <returns>200 OK com mensagem e novo estado; 404 se inexistente.</returns>
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

        /// <summary>
        /// Remove um sensor por ID.
        /// </summary>
        /// <param name="id">ID do sensor.</param>
        /// <returns>204 NoContent; 404 NotFound.</returns>
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

        #endregion

        #region Endpoints REST - Monitorização/diagnóstico

        /// <summary>
        /// Identifica sensores potencialmente avariados por inatividade:
        /// sensores cuja UltimaAtualizacao seja anterior a 24 horas.
        /// </summary>
        /// <returns>Lista de sensores com possível avaria (timeout).</returns>
        // GET: api/Sensores/Avariados
        [HttpGet("Avariados")]
        public async Task<ActionResult<IEnumerable<Sensor>>> GetSensoresAvariados()
        {
            var limite = DateTime.UtcNow.AddHours(-24);
            return await _context.Sensores
                .Where(s => s.UltimaAtualizacao < limite)
                .ToListAsync();
        }

        #endregion
    }
}