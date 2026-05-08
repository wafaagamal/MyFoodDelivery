using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MyFoodDelivery.PublicGateway.Hubs;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add YARP reverse proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add SignalR with Redis backplane for scaling
builder.Services.AddSignalR()
    .AddStackExchangeRedis(builder.Configuration["Redis:Configuration"] ?? "localhost:6379", options =>
    {
        options.Configuration.ChannelPrefix = RedisChannel.Literal("MyFoodDelivery");
    });

// Add Redis connection for location store
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = builder.Configuration["Redis:Configuration"] ?? "localhost:6379";
    return ConnectionMultiplexer.Connect(config);
});

// Add authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Authentication:Authority"];
        options.Audience = builder.Configuration["Authentication:Audience"];
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

        // Configure for SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Add health checks
builder.Services.AddHealthChecks();

// Add Swagger for gateway documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "MyFoodDelivery Public Gateway", 
        Version = "v1",
        Description = "API Gateway for the MyFoodDelivery platform"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();

// Map SignalR hubs
app.MapHub<TrackingHub>("/hubs/tracking");

// Map health check endpoint
app.MapHealthChecks("/health");

// Map YARP reverse proxy
app.MapReverseProxy();

// Simple endpoint for gateway info
app.MapGet("/", () => new
{
    Service = "MyFoodDelivery Public Gateway",
    Version = "1.0.0",
    Endpoints = new[]
    {
        "/api/customers - Customer Service",
        "/api/orders - Ordering Service",
        "/api/restaurants - Restaurant Service",
        "/api/delivery - Delivery Service",
        "/hubs/tracking - Real-time Tracking (SignalR)"
    }
});

app.Run();
