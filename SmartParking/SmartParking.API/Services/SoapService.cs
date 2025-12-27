using Microsoft.EntityFrameworkCore;
using SmartParking.API.Models;

namespace SmartParking.API.Services
{
    public class SoapService : ISoapService
    {
        private readonly ParkingContext _context;

        public SoapService(ParkingContext context)
        {
            _context = context;
        }

        public async Task<List<Parque>> GetParquesXml()
        {
            // 1. AsNoTracking é CRÍTICO: impede o Entity Framework de "colar" 
            // as referências de volta automaticamente.
            var parques = await _context.Parques
                .AsNoTracking()
                .Include(p => p.Lugares) // Inclui os lugares para o XML ficar completo
                .ToListAsync();

            // 2. CIRURGIA DE REMOÇÃO DE CICLOS
            // O SOAP crasha se o Lugar apontar para o Parque. Vamos cortar isso.
            foreach (var parque in parques)
            {
                if (parque.Lugares != null)
                {
                    foreach (var lugar in parque.Lugares)
                    {
                        lugar.Parque = null; // <--- O SEGREDO ESTÁ AQUI
                        
                        // Se tiver Sensores e eles apontarem para o Lugar, corte também:
                        if (lugar.Sensor != null) 
                        {
                            // lugar.Sensor.Lugar = null; // (Descomente se tiver erro no sensor)
                        }
                    }
                }
            }

            return parques;
        }

        public string Ping()
        {
            return "Pong! O serviço SOAP está ativo.";
        }
        
        public async Task<Parque> GetParqueDetalhe(int id)
        {
            var parque = await _context.Parques
                .AsNoTracking()
                .Include(p => p.Lugares)
                .ThenInclude(l => l.Sensor) // Carrega o sensor
                .FirstOrDefaultAsync(p => p.Id == id);

            if (parque == null) return null;

            // LIMPEZA DE CICLOS (Crucial para SOAP)
            if (parque.Lugares != null)
            {
                foreach (var lugar in parque.Lugares)
                {
                    // 1. Corta a ligação Lugar -> Parque
                    lugar.Parque = null; 

                    // 2. Corta a ligação Sensor -> Lugar (Onde está o erro agora)
                    if (lugar.Sensor != null)
                    {
                        lugar.Sensor.Lugar = null; // <--- ISTO RESOLVE O ERRO
                    }
                }
            }

            return parque;
        }
    }
}