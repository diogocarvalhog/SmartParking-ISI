using SmartParking.API.Models;
using System.ServiceModel; // Isto define que é um contrato SOAP

namespace SmartParking.API.Services
{
    [ServiceContract]
    public interface ISoapService
    {
        // Operação 1: Ver todos os parques (O requisito "Data Layer")
        [OperationContract]
        Task<List<Parque>> GetParquesXml();

        // Operação 2: Teste de vida
        [OperationContract]
        string Ping();
        
        [OperationContract]
        Task<Parque> GetParqueDetalhe(int id);
    }
}