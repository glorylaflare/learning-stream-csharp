namespace Person.Infra.Configurations;

public sealed class WorkerSettings
{
    public int IntervaloMinutos { get; init; } = 60;
    public int LimiteRegistros { get; init; } = 10000;
    public bool ApenasRegistrosAtivos { get; init; } = true;
}
