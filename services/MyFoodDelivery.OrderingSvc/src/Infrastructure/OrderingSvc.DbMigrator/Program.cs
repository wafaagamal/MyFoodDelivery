using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderingSvc.Infrastructure.EntityFrameworkCore;
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
    Log.Information("Starting OrderingSvc.DbMigrator...");

    var builder = Host.CreateApplicationBuilder(args);
    
    builder.Services.AddDbContext<OrderingSvcDbContext>(options =>
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
    Log.Fatal(ex, "OrderingSvc.DbMigrator terminated unexpectedly!");
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
            _logger.LogInformation("Starting database migration...");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OrderingSvcDbContext>();

            _logger.LogInformation("Checking for pending migrations...");
            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
            var pendingList = pendingMigrations.ToList();

            if (pendingList.Count > 0)
            {
                _logger.LogInformation("Found {Count} pending migration(s): {Migrations}", 
                    pendingList.Count, string.Join(", ", pendingList));
                
                await dbContext.Database.MigrateAsync(cancellationToken);
                _logger.LogInformation("Database migration completed successfully!");
            }
            else
            {
                _logger.LogInformation("No pending migrations found. Database is up to date.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during database migration!");
            throw;
        }
        finally
        {
            _hostApplicationLifetime.StopApplication();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
