using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OrderingSvc.Application;
using OrderingSvc.HttpApi;
using OrderingSvc.Infrastructure;
using System.Text;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Volo.Abp.Swashbuckle;

namespace OrderingSvc.HttpApi.Host;

[DependsOn(
    typeof(OrderingSvcHttpApiModule),
    typeof(OrderingSvcApplicationModule),
    typeof(OrderingSvcInfrastructureModule),
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpSwashbuckleModule)
)]
public class OrderingSvcHttpApiHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        ConfigureAuthentication(context.Services, configuration);
        ConfigureSwagger(context.Services);
        ConfigureCors(context.Services, configuration);
        
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(OrderingSvcApplicationModule).Assembly);
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
                // For development: accept any token without signature validation
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

    private void ConfigureSwagger(IServiceCollection services)
    {
        services.AddAbpSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Ordering Service API",
                Version = "v1",
                Description = "Orders, Cart and Payment Management API"
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
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Ordering Service API");
        });
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}
