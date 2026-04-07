using Microsoft.Extensions.Logging;
using Person.Domain.Models;
using System.Globalization;
using System.Text;

namespace Person.Infra.Data.Services;

public class CsvGenerateService
{
    private readonly ILogger<CsvGenerateService> _logger;

    public CsvGenerateService(ILogger<CsvGenerateService> logger)
    {
        _logger = logger;
    }

    public async Task GerarCsvStreamingAsync(
        IAsyncEnumerable<Pessoa> pessoaStream,
        Stream outputStream,
        CancellationToken cancellationToken)
    {
        using var write = new StreamWriter(
            stream: outputStream,
            encoding: Encoding.UTF8,
            leaveOpen: true);

        int contador = 0;

        await foreach (var registro in pessoaStream.WithCancellation(cancellationToken))
        {
            var linha = MontarLinhaCsv(registro);
            await write.WriteLineAsync(linha);

            contador++;

            if (contador % 1000 == 0)
            {
                _logger.LogDebug($"Gerades {contador} linhas do CsvGenerate");
            }
        }

        await write.FlushAsync(cancellationToken);
        _logger.LogInformation($"CSV gerado com {contador} registros");
    }

    private string MontarLinhaCsv(Pessoa pessoa)
    {
        var campos = new[]
        {
            EscapeCsvField(pessoa.Id ?? string.Empty),
            EscapeCsvField($"{pessoa.PrimeiroNome} {pessoa.SegundoNome}"),
            pessoa.Idade.ToString(),
            pessoa.DataCadastro.ToString("dd-MM-yyyy HH:mm:ss"),
            pessoa.Salario.ToString("F2", CultureInfo.InvariantCulture),
            pessoa.IsActive.ToString(),
        };

        return string.Join(",", campos);
    }

    public string EscapeCsvField(string campo)
    {
        if (string.IsNullOrEmpty(campo))
            return string.Empty;

        if (campo.Contains(",") || campo.Contains("\"") || campo.Contains("\n") || campo.Contains("\r"))
        {
            return $"\"{campo.Replace("\"", "\"\"")}\"";
        }

        return campo;
    }
}
