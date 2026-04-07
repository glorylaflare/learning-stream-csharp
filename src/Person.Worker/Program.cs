using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Person.Infra.Configurations;
using Person.Infra.Data.Services;
using Person.Worker.Workers;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<MongoDbSettings>(
            context.Configuration.GetSection("MongoDb"));

        services.Configure<SftpSettings>(
            context.Configuration.GetSection("Sftp"));

        services.Configure<WorkerSettings>(
            context.Configuration.GetSection("Worker"));

        services.AddSingleton<MongoDbService>();
        services.AddSingleton<CsvGenerateService>();
        services.AddSingleton<ZipService>();
        services.AddSingleton<SftpService>();

        services.AddHostedService<MongoToSftpWorker>();
    })
    .Build();

await host.RunAsync();