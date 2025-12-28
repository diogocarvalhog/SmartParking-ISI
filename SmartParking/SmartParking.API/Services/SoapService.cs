// -----------------------------------------------------------------------------
// Projeto: SmartParking
// Unidade Curricular: ISI (IPCA)
// Autor: Diogo Graça
// Ficheiro: SoapService.cs
// Descrição: Implementação do serviço SOAP definido em ISoapService.
// Notas:
//  - Usa AsNoTracking para minimizar efeitos de tracking do EF (importante em serialização).
//  - Remove ciclos de referência (Parque <-> Lugar <-> Sensor) para evitar falhas na XML SOAP.
// -----------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using SmartParking.API.Models;

namespace SmartParking.API.Services
{
    #region Serviço SOAP: SoapService

    /// <summary>
    /// Implementação do contrato SOAP (ISoapService).
    /// Responsável por devolver dados em estruturas serializáveis via SOAP/XML.
    /// </summary>
    public class SoapService : ISoapService
    {
        #region Campos privados

        /// <summary>
        /// Contexto EF Core para acesso ao repositório (Azure SQL / SQL Server).
        /// </summary>
        private readonly ParkingContext _context;

        #endregion

        #region Construtor

        /// <summary>
        /// Construtor com injeção do contexto EF.
        /// </summary>
        /// <param name="context">ParkingContext.</param>
        public SoapService(ParkingContext context)
        {
            _context = context;
        }

        #endregion

        #region Operações SOAP

        /// <summary>
        /// Devolve a lista de parques, incluindo lugares (para exportação em XML).
        /// Aplica AsNoTracking e “corte de ciclos” (Lugar -> Parque) para serialização SOAP.
        /// </summary>
        /// <returns>Lista de parques.</returns>
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
                        lugar.Parque = null; // <-- CORTA o ciclo Lugar -> Parque
                    }
                }
            }

            return parques;
        }

        /// <summary>
        /// Operação simples de teste para confirmar que o serviço SOAP está ativo.
        /// </summary>
        /// <returns>Mensagem de resposta (Pong).</returns>
        public string Ping()
        {
            return "Pong! O serviço SOAP está ativo e a funcionar.";
        }

        /// <summary>
        /// Devolve detalhe de um parque, incluindo Lugares e Sensores.
        /// Antes de devolver, remove referências circulares:
        /// - Lugar.Parque = null
        /// - Sensor.Lugar = null
        /// para evitar erros de serialização XML.
        /// </summary>
        /// <param name="id">ID do parque.</param>
        /// <returns>Parque detalhado.</returns>
        public async Task<Parque> GetParqueDetalhe(int id)
        {
            var parque = await _context.Parques
                .AsNoTracking()
                .Include(p => p.Lugares)
                .ThenInclude(l => l.Sensor)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (parque != null && parque.Lugares != null)
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

        #endregion
    }

    #endregion
}