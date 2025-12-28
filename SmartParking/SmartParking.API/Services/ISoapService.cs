// -----------------------------------------------------------------------------
// Projeto: SmartParking
// Unidade Curricular: ISI (IPCA)
// Autor: Diogo Graça
// Ficheiro: ISoapService.cs
// Descrição: Contrato SOAP (ServiceContract) para operações de integração.
// Notas:
//  - Define as operações expostas via SOAP (Data Layer / Exportação XML).
// -----------------------------------------------------------------------------

using SmartParking.API.Models;
using System.ServiceModel; // Isto define que é um contrato SOAP

namespace SmartParking.API.Services
{
    #region Contrato SOAP: ISoapService

    /// <summary>
    /// Contrato de serviço SOAP.
    /// Define operações consumíveis por clientes SOAP.
    /// </summary>
    [ServiceContract]
    public interface ISoapService
    {
        /// <summary>
        /// Operação de exportação: devolve lista de parques (com estrutura apropriada a XML).
        /// </summary>
        /// <returns>Lista de parques.</returns>
        [OperationContract]
        Task<List<Parque>> GetParquesXml();

        /// <summary>
        /// Operação de diagnóstico (heartbeat) para verificar disponibilidade do serviço.
        /// </summary>
        /// <returns>Mensagem de resposta.</returns>
        [OperationContract]
        string Ping();
        
        /// <summary>
        /// Exporta detalhes completos de um parque (incluindo lugares e sensores),
        /// preparado para serialização SOAP.
        /// </summary>
        /// <param name="id">ID do parque.</param>
        /// <returns>Objeto Parque com detalhe.</returns>
        [OperationContract]
        Task<Parque> GetParqueDetalhe(int id);
    }

    #endregion
}