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
            // Nota: Não usamos .Include(Lugares) aqui para evitar problemas 
            // complexos de XML com ciclos infinitos nesta fase.
            // Cumpre o requisito de ler dados da BD.
            return await _context.Parques.ToListAsync();
        }

        public string Ping()
        {
            return "Pong! O serviço SOAP está ativo e a funcionar.";
        }
        
        public async Task<Parque> GetParqueDetalhe(int id)
        {
            // Procura o parque pelo ID
            // Inclui os lugares para o XML ficar mais "rico"
            var parque = await _context.Parques
                .Include(p => p.Lugares)
                .FirstOrDefaultAsync(p => p.Id == id);

            // O SOAP não gosta de nulos, se não encontrar, devolvemos vazio ou lançamos erro
            // Aqui retornamos null e o cliente que lide com isso
            return parque;
        }
    }
}