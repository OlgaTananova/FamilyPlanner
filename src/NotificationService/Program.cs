using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using NotificationService.Consumers;
using NotificationService.Helpers;
using NotificationService.Hubs;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{

    var envFilePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env.prod"));

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


builder.Services.AddControllers();


// Logging and Telemetry
builder.Services.AddApplicationInsightsTelemetry();
builder.Logging.AddApplicationInsights(
    configureTelemetryConfiguration: (config) =>
        { config.ConnectionString = appConfig.ApplicationInsightsConnectionString; },
    configureApplicationInsightsLoggerOptions: (options) =>
    {
        options.IncludeScopes = true;
    });

builder.Services.AddMassTransit(x =>
{
    x.AddConsumersFromNamespaceContaining<CatalogCategoryUpdatedConsumer>();
    x.AddConsumersFromNamespaceContaining<CatalogCategoryCreatedConsumer>();
    x.AddConsumersFromNamespaceContaining<CatalogCategoryDeletedConsumer>();

    x.AddConsumersFromNamespaceContaining<CatalogItemCreatedConsumer>();
    x.AddConsumersFromNamespaceContaining<CatalogItemUpdatedConsumer>();
    x.AddConsumersFromNamespaceContaining<CatalogItemDeletedConsumer>();

    x.AddConsumersFromNamespaceContaining<ShoppingListCreatedConsumer>();
    x.AddConsumersFromNamespaceContaining<ShoppingListDeletedConsumer>();
    x.AddConsumersFromNamespaceContaining<ShoppingListUpdatedConsumer>();

    x.AddConsumersFromNamespaceContaining<ShoppingListItemUpdatedConsumer>();
    x.AddConsumersFromNamespaceContaining<ShoppingListItemsAddedConsumer>();
    x.AddConsumersFromNamespaceContaining<ShoppingListItemDeletedConsumer>();


    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("notifications", false));
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.UseMessageRetry(r =>
        {
            r.Handle<RabbitMqConnectionException>();
            r.Interval(5, TimeSpan.FromSeconds(10));
        });
        cfg.Host(appConfig.RabbitMqHost, "/", h =>
        {
            h.Username(appConfig.RabbitMqUser);
            h.Password(appConfig.RabbitMqPassword);
        });

        cfg.ConfigureEndpoints(context);
    });
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(appConfig.ClientApps); // Allow only these origins
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
        policy.AllowCredentials();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options => { }, options =>
    {
        AppConfig.LoadAzureAdB2CConfig(options, appConfig, builder.Configuration, builder.Environment);
    });


builder.Services.AddScoped<IClaimsTransformation, CustomClaimsTransformation>();
builder.Services.AddAuthorization();

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(20);
    options.HandshakeTimeout = TimeSpan.FromSeconds(30);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}
app.UseWebSockets();
app.UseExceptionHandler(o => { });
app.UseCors("AllowSpecificOrigins");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("notifications");

app.Run();

public partial class Program { }