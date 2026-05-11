using DeliverySvc.Application;
using DeliverySvc.HttpApi;
using DeliverySvc.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Volo.Abp.Swashbuckle;

namespace DeliverySvc.HttpApi.Host;

[DependsOn(
    typeof(DeliverySvcHttpApiModule),
    typeof(DeliverySvcApplicationModule),
    typeof(DeliverySvcInfrastructureModule),
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpAspNetCoreSignalRModule),
    typeof(AbpSwashbuckleModule)
)]
public class DeliverySvcHttpApiHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        ConfigureAuthentication(context.Services, configuration);
        ConfigureSwagger(context.Services);
        ConfigureCors(context.Services, configuration);
        ConfigureSignalR(context.Services, configuration);
        
        // Register data seeder
        context.Services.AddTransient<RiderDataSeeder>();
        
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(DeliverySvcApplicationModule).Assembly);
        });
    }

    private void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = false,
                RequireSignedTokens = false,
                SignatureValidator = (token, _) =>
                {
                    var handler = new Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler();
                    return handler.ReadJsonWebToken(token);
                }
            };
            options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
            {
                OnTokenValidated = ctx =>
                {
                    var identity = ctx.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
                    var sub = identity?.FindFirst("sub")?.Value;
                    if (sub != null && identity?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) == null)
                        identity?.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, sub));
                    return System.Threading.Tasks.Task.CompletedTask;
                }
            };
        });
    }

    private void ConfigureSwagger(IServiceCollection services)
    {
        services.AddAbpSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Delivery Service API",
                Version = "v1",
                Description = "Rider Management and Real-time Tracking API"
            });
            options.DocInclusionPredicate((_, _) => true);
            options.CustomSchemaIds(type => type.FullName);
        });
    }

    private void ConfigureCors(IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .SetIsOriginAllowed(_ => true) // Allow any origin in development
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    private void ConfigureSignalR(IServiceCollection services, IConfiguration configuration)
    {
        // SignalR Redis backplane for scaling (requires Microsoft.AspNetCore.SignalR.StackExchangeRedis package)
        // Disabled for initial development - enable when deploying to multiple instances
        services.AddSignalR();
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseCorrelationId();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Delivery Service API");
        });
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints(endpoints =>
        {
            endpoints.MapHub<TrackingHub>("/hubs/tracking");
        });

        // Seed data in development
        if (env.IsDevelopment())
        {
            try
            {
                using var scope = context.ServiceProvider.CreateScope();
                var seeder = scope.ServiceProvider.GetRequiredService<RiderDataSeeder>();
                seeder.SeedAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                var logger = context.ServiceProvider.GetRequiredService<ILogger<DeliverySvcHttpApiHostModule>>();
                logger.LogWarning(ex, "Rider seeding failed - database migration may be needed");
            }
        }
    }
}
