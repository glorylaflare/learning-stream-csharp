namespace Person.Infra.Configurations;

public sealed class SftpSettings
{
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 22;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string PrivateKeyPath { get; init; } = string.Empty; // Opcional
    public string RemoteDirectory { get; init; } = string.Empty;
}
