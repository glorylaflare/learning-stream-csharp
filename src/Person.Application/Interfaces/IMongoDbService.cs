using Person.Domain.Models;

namespace Person.Application.Interfaces;

public interface IMongoDbService
{
    Task<List<Pessoa>> BuscarDadosAsync(DateTime? dataInicio = null, int limite = 10000);
    Task<long> ContarRegistrosAsync(DateTime? dataInicio = null);
}
