using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartParking.API.Models;
using SmartParking.API.Services; // Necessário para a Meteorologia

namespace SmartParking.API.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão dos Parques de Estacionamento.
    /// Inclui operações CRUD, Dashboard de ocupação e integração com Meteorologia.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ParquesController : ControllerBase
    {
        #region Propriedades e Construtor

        private readonly ParkingContext _context;
        private readonly WeatherService _weatherService;

        public ParquesController(ParkingContext context, WeatherService weatherService)
        {
            _context = context;
            _weatherService = weatherService;
        }

        #endregion

        #region CRUD (Create, Read, Update, Delete)

        /// <summary>
        /// Lista todos os parques registados na base de dados.
        /// </summary>
        /// <returns>Uma lista de objetos Parque.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Parque>>> GetParques()
        {
            return await _context.Parques.ToListAsync();
        }

        /// <summary>
        /// Obtém os detalhes de um parque específico pelo seu ID.
        /// </summary>
        /// <param name="id">O identificador do parque.</param>
        /// <returns>O objeto Parque ou NotFound.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Parque>> GetParque(int id)
        {
            var parque = await _context.Parques.FindAsync(id);

            if (parque == null)
            {
                return NotFound();
            }

            return parque;
        }

        /// <summary>
        /// Cria um novo parque.
        /// </summary>
        /// <param name="parque">O objeto Parque a criar.</param>
        /// <returns>O parque criado.</returns>
        [HttpPost]
        public async Task<ActionResult<Parque>> PostParque(Parque parque)
        {
            _context.Parques.Add(parque);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetParque", new { id = parque.Id }, parque);
        }

        /// <summary>
        /// Atualiza os dados de um parque existente.
        /// </summary>
        /// <param name="id">O ID do parque a atualizar.</param>
        /// <param name="parque">Os novos dados do parque.</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutParque(int id, Parque parque)
        {
            if (id != parque.Id)
            {
                return BadRequest();
            }

            _context.Entry(parque).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ParqueExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Elimina um parque da base de dados.
        /// </summary>
        /// <param name="id">O ID do parque a apagar.</param>
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

        #endregion

        #region Funcionalidades Extra (Dashboard e Pesquisa)

        /// <summary>
        /// Dashboard: Calcula a ocupação atual do parque (Livres vs Ocupados).
        /// </summary>
        /// <param name="id">ID do parque.</param>
        /// <returns>Um resumo estatístico da ocupação.</returns>
        [HttpGet("{id}/Estado")]
        public async Task<ActionResult<object>> GetEstadoParque(int id)
        {
            var parque = await _context.Parques
                .Include(p => p.Lugares)
                .ThenInclude(l => l.Sensor)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (parque == null) return NotFound();

            int totalLugares = parque.Lugares.Count;
            // Conta lugares onde o sensor existe E o estado é true (ocupado)
            int lugaresOcupados = parque.Lugares.Count(l => l.Sensor != null && l.Sensor.Estado == true);
            int lugaresLivres = totalLugares - lugaresOcupados;

            return new
            {
                ParqueId = parque.Id,
                Nome = parque.Nome,
                Capacidade = totalLugares,
                Ocupados = lugaresOcupados,
                Livres = lugaresLivres,
                EstaCheio = lugaresLivres == 0,
                DataConsulta = DateTime.Now
            };
        }

        /// <summary>
        /// Pesquisa parques por nome ou localização (ex: "Barcelos").
        /// </summary>
        /// <param name="termo">Palavra a pesquisar.</param>
        /// <returns>Lista de parques encontrados.</returns>
        [HttpGet("Pesquisa")]
        public async Task<ActionResult<IEnumerable<Parque>>> PesquisarParques([FromQuery] string termo)
        {
            if (string.IsNullOrEmpty(termo))
            {
                return BadRequest("Tens de escrever algo para pesquisar.");
            }

            var resultados = await _context.Parques
                .Where(p => p.Nome.Contains(termo) || p.Localizacao.Contains(termo))
                .Include(p => p.Lugares)
                .ToListAsync();

            if (!resultados.Any())
            {
                return NotFound($"Não encontrei parques com a palavra '{termo}'.");
            }

            return resultados;
        }

        #endregion

        #region Integrações Externas (OpenWeatherMap)

        /// <summary>
        /// Obtém informação do parque enriquecida com dados meteorológicos em tempo real.
        /// Requisito: Utilização de serviços web externos.
        /// </summary>
        /// <param name="id">ID do parque.</param>
        /// <returns>Dados do parque e temperatura atual.</returns>
        [HttpGet("{id}/Info")]
        public async Task<ActionResult<object>> GetParqueComMeteo(int id)
        {
            var parque = await _context.Parques.FindAsync(id);
            if (parque == null) return NotFound();

            // Chama o serviço externo injectado
            string tempo = await _weatherService.GetTempoInfo(parque.Localizacao);

            return Ok(new
            {
                Id = parque.Id,
                Nome = parque.Nome,
                Localizacao = parque.Localizacao,
                Capacidade = parque.CapacidadeTotal,
                Meteorologia = tempo,
                Nota = "Dados meteorológicos fornecidos por OpenWeatherMap API"
            });
        }

        #endregion

        #region Helpers

        private bool ParqueExists(int id)
        {
            return _context.Parques.Any(e => e.Id == id);
        }

        #endregion
    }
}