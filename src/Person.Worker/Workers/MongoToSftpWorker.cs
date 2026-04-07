using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Person.Domain.Models;
using Person.Infra.Configurations;
using Person.Infra.Data.Services;
using System.Runtime.CompilerServices;

namespace Person.Worker.Workers;

public class MongoToSftpWorker : BackgroundService
{
    private readonly ILogger<MongoToSftpWorker> _logger;
    private readonly MongoDbService _mongoDbService;
    private readonly CsvGenerateService _csvGenerator;
    private readonly ZipService _zipService;
    private readonly SftpService _sftpService;
    private readonly WorkerSettings _settings;
    private readonly MongoDbSettings _mongoDbSettings;

    public MongoToSftpWorker(
        ILogger<MongoToSftpWorker> logger,
        MongoDbService mongoDbService,
        CsvGenerateService csvGenerator,
        ZipService zipService,
        SftpService sftpService,
        IOptions<MongoDbSettings> mongoDbSettings,
        IOptions<WorkerSettings> settings)
    {
        _logger = logger;
        _mongoDbService = mongoDbService;
        _csvGenerator = csvGenerator;
        _zipService = zipService;
        _sftpService = sftpService;
        _settings = settings.Value;
        _mongoDbSettings = mongoDbSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker MongoDB → CSV → ZIP → SFTP iniciado");

        await ProcessarAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(_settings.IntervaloMinutos), stoppingToken);
                await ProcessarAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no ciclo de processamento");
            }
        }
    }

    private async Task ProcessarAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando processamento...");

        var totalRegistros = await _mongoDbService.ContarRegistrosAsync();

        if (totalRegistros == 0)
        {
            _logger.LogWarning("Nenhum registro encontrado no MongoDB");
            return;
        }

        _logger.LogInformation($"Total de registros a processar: {totalRegistros}");

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var nomeZip = $"dados_exportacao_{timestamp}.zip";

        using var zipStream = new MemoryStream();

        await _zipService.CriarZipStreamingAsync(
            async (csvStream) =>
            {
                var dadosStream = BuscarDadosStreamingAsync(cancellationToken);
                await _csvGenerator.GerarCsvStreamingAsync(dadosStream, csvStream, cancellationToken);
            },
            zipStream,
            $"dados_{timestamp}.csv",
            cancellationToken
        );

        await _sftpService.EnviarArquivoStreamingAsync(zipStream, nomeZip, cancellationToken);

        _logger.LogInformation($"Processamento concluído! Arquivo enviado: {nomeZip}");
    }

    private async IAsyncEnumerable<Pessoa> BuscarDadosStreamingAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var client = new MongoClient(_mongoDbSettings.ConnectionString);
        var database = client.GetDatabase(_mongoDbSettings.DatabaseName);
        var collection = database.GetCollection<Pessoa>(_mongoDbSettings.CollectionName);

        var filter = Builders<Pessoa>.Filter.Eq(x => x.IsActive, true);

        using var cursor = await collection.FindAsync(filter, cancellationToken: cancellationToken);

        while (await cursor.MoveNextAsync(cancellationToken))
        {
            foreach (var documento in cursor.Current)
            {
                yield return documento;
            }
        }
    }
}
