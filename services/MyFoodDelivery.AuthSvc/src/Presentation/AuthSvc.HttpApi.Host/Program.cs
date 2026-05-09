using AuthSvc.HttpApi.Host;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Async(c => c.Console())
    .WriteTo.Async(c => c.File("Logs/authsvc-.log", rollingInterval: RollingInterval.Day))
    .CreateLogger();

try
{
    Log.Information("Starting AuthSvc.HttpApi.Host...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host
        .UseAutofac()
        .UseSerilog();

    await builder.AddApplicationAsync<AuthSvcHttpApiHostModule>();

    var app = builder.Build();
    await app.InitializeApplicationAsync();
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "AuthSvc.HttpApi.Host terminated unexpectedly!");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}
