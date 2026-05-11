using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using RestaurantSvc.Application;
using RestaurantSvc.HttpApi;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Volo.Abp.Swashbuckle;

namespace RestaurantSvc.HttpApi.Host;

[DependsOn(
    typeof(RestaurantSvcHttpApiModule),
    typeof(RestaurantSvcApplicationModule),
    typeof(RestaurantSvcMongoDbModule),
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpSwashbuckleModule)
)]
public class RestaurantSvcHttpApiHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        ConfigureAuthentication(context.Services, configuration);
        ConfigureMongoDB(context.Services, configuration);
        ConfigureSwagger(context.Services);
        ConfigureCors(context.Services, configuration);
        
        // Register data seeder
        context.Services.AddTransient<RestaurantDataSeeder>();
        
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(RestaurantSvcApplicationModule).Assembly);
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
                // Accept any token without signature validation (development only)
                SignatureValidator = (token, _) =>
                {
                    var handler = new Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler();
                    var jwt = handler.ReadJsonWebToken(token);
                    return jwt;
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

    private void ConfigureMongoDB(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDB") 
            ?? "mongodb://localhost:27017/MyFoodDelivery_Restaurant";
        var mongoUrl = new MongoUrl(connectionString);
        var client = new MongoClient(mongoUrl);
        var database = client.GetDatabase(mongoUrl.DatabaseName ?? "MyFoodDelivery_Restaurant");
        
        services.AddSingleton<IMongoClient>(client);
        services.AddSingleton(database);
        
        // Configure ABP connection string for MongoDB module
        Configure<Volo.Abp.Data.AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings.Default = connectionString;
        });
    }

    private void ConfigureSwagger(IServiceCollection services)
    {
        services.AddAbpSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Restaurant Service API",
                Version = "v1",
                Description = "Restaurant and Menu Management API"
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
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Restaurant Service API");
        });
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();

        // Seed data in development
        if (env.IsDevelopment())
        {
            using var scope = context.ServiceProvider.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<RestaurantDataSeeder>();
            seeder.SeedAsync().GetAwaiter().GetResult();
        }
    }
}
