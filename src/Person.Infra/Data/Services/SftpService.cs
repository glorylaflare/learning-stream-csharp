using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Person.Infra.Configurations;
using Renci.SshNet;

namespace Person.Infra.Data.Services;

public class SftpService
{
    private readonly SftpSettings _settings;
    private readonly ILogger<SftpService> _logger;
    private SftpClient? _sftpClient;

    public SftpService(IOptions<SftpSettings> settings, ILogger<SftpService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    private async Task ConectarAsync(CancellationToken cancellationToken)
    {
        try
        {
            var authenticationMethods = new List<AuthenticationMethod>();

            if (!string.IsNullOrEmpty(_settings.Password))
            {
                authenticationMethods.Add(new PasswordAuthenticationMethod(_settings.Username, _settings.Password));
            }

            if (!string.IsNullOrEmpty(_settings.PrivateKeyPath))
            {
                var privateKeyFile = new PrivateKeyFile(_settings.PrivateKeyPath);
                authenticationMethods.Add(new PrivateKeyAuthenticationMethod(_settings.Username, privateKeyFile));
            }

            var connectionInfo = new ConnectionInfo(
                _settings.Host, 
                _settings.Port, 
                _settings.Username,
                authenticationMethods.ToArray());

            _sftpClient = new SftpClient(connectionInfo);
            await Task.Run(() => _sftpClient.Connect(), cancellationToken);

            _logger.LogInformation($"Conectado ao SFTP: {_settings.Host}:{_settings.Port}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao conectar ao SFTP");
            throw;
        }
    }

    public async Task EnviarArquivoStreamingAsync(
        Stream fileStream,
        string nomeArquivoRemoto,
        CancellationToken cancellationToken)
    {
        if (_sftpClient == null || !_sftpClient.IsConnected)
        {
            await ConectarAsync(cancellationToken);
        }

        var caminhoCompleto = Path.Combine(_settings.RemoteDirectory, nomeArquivoRemoto).Replace('\\', '/');

        try
        {
            fileStream.Position = 0;

            await Task.Run(() =>
            {
                using (var remoteStream = _sftpClient.OpenWrite(caminhoCompleto))
                {
                    fileStream.CopyTo(remoteStream);
                    remoteStream.Flush();
                }
            }, cancellationToken);

            _logger.LogInformation($"Arquivo enviado via SFTP: {caminhoCompleto} ({fileStream.Length} bytes)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar arquivo via SFTP");
            throw;
        }
    }

    public async Task<List<string>> ListarArquivosRemotosAsync(string? path = null, CancellationToken cancellationToken = default)
    {
        if (_sftpClient == null || !_sftpClient.IsConnected)
        {
            await ConectarAsync(cancellationToken);
        }

        var remotePath = path ?? _settings.RemoteDirectory;

        var arquivos = await Task.Run(() => _sftpClient.ListDirectory(remotePath)
            .Where(f => !f.IsDirectory)
            .Select(f => f.Name)
            .ToList(), cancellationToken);

        return arquivos;
    }

    public void Dispose()
    {
        _sftpClient?.Disconnect();
        _sftpClient?.Dispose();
    }
}
