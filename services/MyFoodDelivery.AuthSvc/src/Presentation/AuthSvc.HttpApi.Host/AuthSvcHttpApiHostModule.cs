using AuthSvc.HttpApi;
using AuthSvc.HttpApi.Host.DataSeeders;
using AuthSvc.Infrastructure;
using OpenIddict.Server;
using Volo.Abp;
using Volo.Abp.Account;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.EventBus.RabbitMq;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Abp.Swashbuckle;

namespace AuthSvc.HttpApi.Host;

[DependsOn(
    typeof(AuthSvcHttpApiModule),
    typeof(AuthSvcInfrastructureModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpOpenIddictAspNetCoreModule),
    typeof(AbpAspNetCoreMvcModule),
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpEventBusRabbitMqModule)
)]
public class AuthSvcHttpApiHostModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<OpenIddictServerBuilder>(builder =>
        {
            builder.SetTokenEndpointUris("/connect/token");
            builder.SetUserinfoEndpointUris("/connect/userinfo");

            builder.AllowPasswordFlow();
            builder.AllowRefreshTokenFlow();

            builder.SetAccessTokenLifetime(TimeSpan.FromHours(1));
            builder.SetRefreshTokenLifetime(TimeSpan.FromDays(30));

            builder.AddDevelopmentEncryptionCertificate();
            builder.AddDevelopmentSigningCertificate();

            builder.UseAspNetCore()
                .EnableTokenEndpointPassthrough()
                .EnableUserinfoEndpointPassthrough()
                .DisableTransportSecurityRequirement();
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        ConfigureSwagger(context.Services);
        ConfigureCors(context.Services, configuration);

        context.Services.AddTransient<AuthDataSeeder>();
    }

    private static void ConfigureSwagger(IServiceCollection services)
    {
        services.AddAbpSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "AuthSvc API",
                Version = "v1"
            });
            options.DocInclusionPredicate((_, _) => true);
            options.CustomSchemaIds(type => type.FullName);
        });
    }

    private static void ConfigureCors(IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                var origins = configuration.GetSection("App:CorsOrigins").Get<string[]>()
                              ?? Array.Empty<string>();

                policy.WithOrigins(origins)
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
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "AuthSvc API");
        });

        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }

    public override async Task OnPostApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        using var scope = context.ServiceProvider.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<AuthDataSeeder>();
        await seeder.SeedAsync();
    }
}
