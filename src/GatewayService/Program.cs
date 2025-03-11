using Microsoft.Identity.Web;
using AspNetCoreRateLimit;
using GatewayService.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;


var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{

    var envFilePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env"));

    if (File.Exists(envFilePath))
    {
        DotNetEnv.Env.Load(envFilePath);
        Console.WriteLine(".env.prod file loaded successfully!");
    }
    else
    {
        Console.WriteLine($".env.prod file not found at: {envFilePath}");
    }
}

// Load environment-specific configuration into AppConfig
var appConfig = AppConfig.LoadConfiguration(builder.Configuration, builder.Environment);

// Register AppConfig as a singleton
builder.Services.AddSingleton(appConfig);

// Logging and Telemetry
builder.Services.AddApplicationInsightsTelemetry();
builder.Logging.AddApplicationInsights(
    configureTelemetryConfiguration: (config) =>
        {
            config.ConnectionString = appConfig.ApplicationInsightsConnectionString;
        },
    configureApplicationInsightsLoggerOptions: (options) =>
    {
        options.IncludeScopes = true;
    });

// Add authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options => { }, options =>
    {
        AppConfig.LoadAzureAdB2CConfig(options, appConfig, builder.Configuration, builder.Environment);
    });

// Add authorization
builder.Services.AddAuthorization();

// Add configuration for rate limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

// Cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("customPolicy", b =>
    {
        b.AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .WithOrigins(appConfig.ClientApps);
    });
});

// Add YARP reverse proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseCors("customPolicy");
app.UseIpRateLimiting(); // Add Rate Limiting Middleware
app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();
app.Run();
