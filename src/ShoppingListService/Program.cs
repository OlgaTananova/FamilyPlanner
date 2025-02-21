using System.Diagnostics;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using ShoppingListService.Consumers;
using ShoppingListService.Data;
using ShoppingListService.Helpers;
using ShoppingListService.Services;

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

// Add Telemetry and logging
builder.Services.AddApplicationInsightsTelemetry();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddApplicationInsights(configureTelemetryConfiguration: (config) =>
{
    config.ConnectionString = appConfig.ApplicationInsightsConnectionString;
},
configureApplicationInsightsLoggerOptions: (options) =>
{
    options.IncludeScopes = true;
});

builder.Services.AddControllers();

// Database context
builder.Services.AddDbContext<ShoppingListContext>(opt =>
{
    opt.UseNpgsql(appConfig.DefaultConnectionString);
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance =
            $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

        context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
#nullable enable
        Activity? activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
        context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
    });

// RabbitMq Messager
builder.Services.AddMassTransit(x =>
{
    x.AddConsumersFromNamespaceContaining<CatalogItemCreatedConsumer>();
    x.AddConsumersFromNamespaceContaining<CatalogItemUpdatedConsumer>();
    x.AddConsumersFromNamespaceContaining<CatalogCategoryUpdatedConsumer>();
    x.AddConsumersFromNamespaceContaining<CatalogItemDeletedConsumer>();
    x.AddConsumersFromNamespaceContaining<CatalogItemSeededConsumer>();

    x.AddEntityFrameworkOutbox<ShoppingListContext>(o =>
           {
               o.QueryDelay = TimeSpan.FromSeconds(10);
               o.UsePostgres();
               o.UseBusOutbox();
           });

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("shoppinglist", false));
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

        cfg.ReceiveEndpoint("shoppinglist-catalog-item-created", e =>
        {
            // if the db is down the massage bus will retry to deliver the message 5 times with an interval of 5 sec
            e.UseMessageRetry(r => r.Interval(5, 5));
            e.ConfigureConsumer<CatalogItemCreatedConsumer>(context);
        });
        cfg.ReceiveEndpoint("shoppinglist-catalog-category-updated", e =>
        {
            // if the db is down the massage bus will retry to deliver the message 5 times with an interval of 5 sec
            e.UseMessageRetry(r => r.Interval(5, 5));
            e.ConfigureConsumer<CatalogCategoryUpdatedConsumer>(context);
        });
        cfg.ReceiveEndpoint("shoppinglist-catalog-item-updated", e =>
        {
            // if the db is down the massage bus will retry to deliver the message 5 times with an interval of 5 sec
            e.UseMessageRetry(r => r.Interval(5, 5));
            e.ConfigureConsumer<CatalogItemUpdatedConsumer>(context);
        });
        cfg.ReceiveEndpoint("shoppinglist-catalog-item-deleted", e =>
        {
            // if the db is down the massage bus will retry to deliver the message 5 times with an interval of 5 sec
            e.UseMessageRetry(r => r.Interval(5, 5));
            e.ConfigureConsumer<CatalogItemDeletedConsumer>(context);
        });
        cfg.ReceiveEndpoint("shoppinglist-catalog-item-seeded", e =>
        {
            // if the db is down the massage bus will retry to deliver the message 5 times with an interval of 5 sec
            e.UseMessageRetry(r => r.Interval(5, 5));
            e.ConfigureConsumer<CatalogItemSeededConsumer>(context);
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(appConfig.ClientApps) // Allow only these origins
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});


// Configure authentication with Azure AD B2C
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options => { }, options =>
    {
        AppConfig.LoadAzureAdB2CConfig(options, appConfig, builder.Configuration, builder.Environment);
    });

builder.Services.AddScoped<IClaimsTransformation, CustomClaimsTransformation>();
builder.Services.AddScoped<IShoppingListService, ShoppingListService.Data.ShoppingListService>();
builder.Services.AddScoped<IShoppingListBusinessService, ShoppingListBusinessService>();
builder.Services.AddScoped<IRequestContextService, RequestContextService>();
builder.Services.AddHttpContextAccessor();

// Add authorization
builder.Services.AddAuthorization(options =>
{
    // IsAdmin Policy
    options.AddPolicy("IsAdmin", policy =>
    {
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "admin" && c.Value == "true"));
    });

    // IsAdult Policy
    options.AddPolicy("IsAdult", policy =>
    {
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "role" &&
                (c.Value == "Parent" || c.Value == "Other member")));
    });
}
);
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}
app.UseExceptionHandler(o => { });
app.UseStatusCodePages(); // Activate problem details middleware
app.UseCors("AllowSpecificOrigins");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    DbInitializer.AddSearchingFunction(app);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

app.Run();

public partial class Program { }



