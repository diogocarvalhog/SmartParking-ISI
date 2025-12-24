using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartParking.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace SmartParking.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LugaresController : ControllerBase
    {
        private readonly ParkingContext _context;

        public LugaresController(ParkingContext context)
        {
            _context = context;
        }

        // GET: api/Lugares
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Lugar>>> GetLugares()
        {
            // Adicionamos .Include(l => l.Sensor) para trazer os dados do sensor junto
            return await _context.Lugares
                .Include(l => l.Sensor) 
                .ToListAsync();
        }

        // GET: api/Lugares/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Lugar>> GetLugar(int id)
        {
            var lugar = await _context.Lugares.Include(l => l.Parque).FirstOrDefaultAsync(l => l.Id == id);

            if (lugar == null)
            {
                return NotFound();
            }

            return lugar;
        }

        // GET: api/Lugares/Parque/1 (Bónus: Todos os lugares do Parque X)
        [HttpGet("Parque/{parqueId}")]
        public async Task<ActionResult<IEnumerable<Lugar>>> GetLugaresPorParque(int parqueId)
        {
            return await _context.Lugares
                .Where(l => l.ParqueId == parqueId)
                .ToListAsync();
        }

        // POST: api/Lugares
        [HttpPost]
        public async Task<ActionResult<Lugar>> PostLugar(Lugar lugar)
        {
            _context.Lugares.Add(lugar);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLugar", new { id = lugar.Id }, lugar);
        }

        // DELETE: api/Lugares/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLugar(int id)
        {
            var lugar = await _context.Lugares.FindAsync(id);
            if (lugar == null)
            {
                return NotFound();
            }

            _context.Lugares.Remove(lugar);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        
        // POST: api/Lugares/1/Ocupar
        [HttpPost("{id}/Ocupar")]
        public async Task<IActionResult> OcuparLugar(int id)
        {
            // 1. Vai buscar o sensor deste lugar
            var sensor = await _context.Sensores.FirstOrDefaultAsync(s => s.LugarId == id);
    
            if (sensor == null) return NotFound("Este lugar não tem sensores instalados.");

            // 2. Atualiza o estado
            sensor.Estado = true; // Ocupado
            sensor.UltimaAtualizacao = DateTime.UtcNow; // Regista a hora

            // 3. Guarda
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Lugar {id} foi ocupado às {sensor.UltimaAtualizacao}" });
        }

        // POST: api/Lugares/1/Libertar
        [HttpPost("{id}/Libertar")]
        public async Task<IActionResult> Libertar(int id)
        {
            // 1. Procurar o Sensor associado a este Lugar
            var sensor = await _context.Sensores.FirstOrDefaultAsync(s => s.LugarId == id);

            if (sensor == null)
            {
                return NotFound("Sensor não encontrado para este lugar.");
            }

            // 2. Verificar se já estava livre (Regra de Robustez)
            if (sensor.Estado == false)
            {
                return BadRequest("O lugar já está livre! Não há valor a pagar.");
            }

            // 3. Calcular o Tempo
            // Se a data for nula (erro de sistema), assumimos a hora atual (o que dará custo 0)
            DateTime horaEntrada = sensor.UltimaAtualizacao ?? DateTime.Now;
            DateTime horaSaida = DateTime.Now;
            TimeSpan duracao = horaSaida - horaEntrada;

            // 4. Calcular o Preço (Vamos definir 2.00€ por hora, por exemplo)
            double precoPorHora = 2.00;
            // Usamos TotalHours para cobrar frações (ex: 30 min = 0.5 horas * 2€ = 1€)
            double valorAPagar = duracao.TotalHours * precoPorHora; 

            // 5. Atualizar o Sensor para Livre
            sensor.Estado = false;
            sensor.UltimaAtualizacao = horaSaida; // Atualizamos para saber quando saiu
    
            // Salvar na base de dados
            await _context.SaveChangesAsync();

            // 6. Retornar a "Fatura" em JSON
            return Ok(new
            {
                message = $"Lugar {id} libertado com sucesso.",
                horaEntrada = horaEntrada,
                horaSaida = horaSaida,
                tempoTotal = $"{duracao.Hours}h {duracao.Minutes}m {duracao.Seconds}s",
                precoPorHora = $"{precoPorHora}€",
                totalPagar = $"{valorAPagar.ToString("F2")}€" // F2 formata para 2 casas decimais
            });
        }
        // GET: api/Lugares/Livres
        // Mostra apenas lugares que estão desocupados
        [HttpGet("Livres")]
        public async Task<ActionResult<IEnumerable<object>>> GetLugaresLivres()
        {
            var lugaresLivres = await _context.Lugares
                .Include(l => l.Sensor)
                .Where(l => l.Sensor != null && l.Sensor.Estado == false) // Tem sensor E está livre
                .Select(l => new 
                {
                    Id = l.Id,
                    Numero = l.NumeroLugar,
                    Piso = l.Piso,
                    Parque = l.Parque.Nome
                })
                .ToListAsync();

            return Ok(lugaresLivres);
        }
        
        // GET: api/Lugares/1/Checkout
        // Simula a saída e calcula o preço a pagar
        [HttpGet("{id}/Checkout")]
        public async Task<ActionResult<object>> CheckoutSimulado(int id)
        {
            var sensor = await _context.Sensores
                .Include(s => s.Lugar)
                .ThenInclude(l => l.Parque) // Para saber o nome do parque
                .FirstOrDefaultAsync(s => s.LugarId == id);

            if (sensor == null) return NotFound("Sensor não encontrado.");
            if (sensor.Estado == false) return BadRequest("O lugar já está livre. Não há nada a pagar.");

            // 1. Calcular tempo decorrido
            DateTime entrada = sensor.UltimaAtualizacao ?? DateTime.UtcNow;
            DateTime saida = DateTime.UtcNow;
            TimeSpan duracao = saida - entrada;

            // 2. Regra de Negócio: Preço (Ex: 1.50€ por hora)
            double precoPorHora = 1.50;
            double totalPagar = duracao.TotalHours * precoPorHora;

            // Arredondar a 2 casas decimais
            totalPagar = Math.Round(totalPagar, 2);

            // Se foi menos de 15 min, é grátis (exemplo de regra extra)
            if (duracao.TotalMinutes < 15) totalPagar = 0.0;

            return Ok(new
            {
                Lugar = sensor.Lugar.NumeroLugar,
                Parque = sensor.Lugar.Parque.Nome,
                TempoEstacionado = $"{duracao.Hours}h {duracao.Minutes}m",
                Entrada = entrada,
                Saida = saida,
                PrecoPorHora = $"{precoPorHora}€",
                TotalAPagar = $"{totalPagar}€"
            });
        }
        
    }
}