using DnsClient.Internal;
using Microsoft.Extensions.Logging;
using System.IO.Compression;

namespace Person.Infra.Data.Services;

public class ZipService
{
    private readonly ILogger<ZipService> _logger;

    public ZipService(ILogger<ZipService> logger)
    {
        _logger = logger;
    }

    public async Task CriarZipStreamingAsync(
        Func<Stream, Task> gerarCsvCallback,
        Stream outputZipStream,
        string nomeArquivoCsv,
        CancellationToken cancellationToken)
    {
        using var zipArchive = new ZipArchive(outputZipStream, ZipArchiveMode.Create, true);

        var zipEntry = zipArchive.CreateEntry(nomeArquivoCsv, CompressionLevel.Optimal);

        using var entryStream = zipEntry.Open();

        await gerarCsvCallback(entryStream);

        _logger.LogInformation($"ZIP criado com arquivo: {nomeArquivoCsv}");
    }
}
