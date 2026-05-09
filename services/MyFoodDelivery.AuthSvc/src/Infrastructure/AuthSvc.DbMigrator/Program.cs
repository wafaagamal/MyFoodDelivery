using AuthSvc.Infrastructure.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/migrations-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting AuthSvc.DbMigrator...");

    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddDbContext<AuthSvcDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("Default");
        options.UseSqlServer(connectionString);
    });

    builder.Services.AddHostedService<DbMigratorHostedService>();
    builder.Services.AddSerilog();

    var app = builder.Build();
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "AuthSvc.DbMigrator terminated unexpectedly!");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

public class DbMigratorHostedService : IHostedService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DbMigratorHostedService> _logger;

    public DbMigratorHostedService(
        IHostApplicationLifetime hostApplicationLifetime,
        IServiceProvider serviceProvider,
        ILogger<DbMigratorHostedService> logger)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting AuthSvc database migration...");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AuthSvcDbContext>();

            _logger.LogInformation("Ensuring database is created and all tables are up to date...");
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);

            _logger.LogInformation("AuthSvc database migration completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during AuthSvc database migration.");
            throw;
        }
        finally
        {
            _hostApplicationLifetime.StopApplication();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
