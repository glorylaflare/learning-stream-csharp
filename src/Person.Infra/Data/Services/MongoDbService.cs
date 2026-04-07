using DnsClient.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Person.Application.Interfaces;
using Person.Domain.Models;
using Person.Infra.Configurations;

namespace Person.Infra.Data.Services;

public class MongoDbService : IMongoDbService
{
    private readonly IMongoCollection<Pessoa> _collection;
    private readonly ILogger<MongoDbService> _logger;

    public MongoDbService(IOptions<MongoDbSettings> settings, ILogger<MongoDbService> logger)
    {
        _logger = logger;
        var mongoSettings = settings.Value;
        var client = new MongoClient(mongoSettings.ConnectionString);
        var database = client.GetDatabase(mongoSettings.DatabaseName);
        _collection = database.GetCollection<Pessoa>(mongoSettings.CollectionName);
    }

    public async Task<long> ContarRegistrosAsync(DateTime? dataInicio = null)
    {
        var filterBuilder = Builders<Pessoa>.Filter;
        var filter = filterBuilder.Empty;

        if (dataInicio.HasValue)
        {
            filter = filterBuilder.Gte(x => x.DataCadastro, dataInicio.Value);
        }

        filter = filterBuilder.And(filter, filterBuilder.Eq(x => x.IsActive, true));

        return await _collection.CountDocumentsAsync(filter);
    }

    public async Task<List<Pessoa>> BuscarDadosAsync(DateTime? dataInicio = null, int limite = 10000)
    {
        try
        {
            var filterBuilder = Builders<Pessoa>.Filter;
            var filter = filterBuilder.Empty;

            if (dataInicio.HasValue)
            {
                filter = filterBuilder.Gte(x => x.DataCadastro, dataInicio.Value);
            }

            filter = filterBuilder.And(filter, filterBuilder.Eq(x => x.IsActive, true));

            var dados = await _collection
                .Find(filter)
                .Limit(limite)
                .SortByDescending(x => x.DataCadastro)
                .ToListAsync();

            _logger.LogInformation($"Buscados {dados.Count} registros do MongoDb");

            return dados;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar dados do MongoDb");
            throw;
        }
    }
}
