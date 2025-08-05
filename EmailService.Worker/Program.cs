using EmailService.Application;
using EmailService.Infrastructure;
using EmailService.Worker.Extensions;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        IConfiguration configuration = context.Configuration;

        _ = services.AddInfrastructure(configuration);
        _ = services.AddApplicationServices();
        _ = services.AddWorkerServices();
    })
    .Build();

await host.RunAsync();
