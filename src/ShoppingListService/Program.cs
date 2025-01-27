using Contracts.Catalog;
using DotNetEnv;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using ShoppingListService.Consumers;
using ShoppingListService.Data;
using ShoppingListService.Helpers;

var builder = WebApplication.CreateBuilder(args);


// Add Telemetry and logging
builder.Services.AddApplicationInsightsTelemetry();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddApplicationInsights(configureTelemetryConfiguration: (config) =>
{
    config.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
},
configureApplicationInsightsLoggerOptions: (options) =>
{
    options.IncludeScopes = true; // Optional: Enable scopes for structured logging
});

builder.Services.AddControllers();

// Database context
builder.Services.AddDbContext<ShoppingListContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
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
        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMq:User"]);
            h.Password(builder.Configuration["RabbitMq:Password"]);
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
        policy.WithOrigins(builder.Configuration.GetSection("ClientApps").Get<string[]>()) // Allow only these origins
              .AllowAnyHeader()            // Allow any headers
              .AllowAnyMethod()           // Allow any HTTP methods
              .AllowCredentials();
    });
});

// Configure authentication with Azure AD B2C

builder.Services.AddAuthentication("Bearer")
    .AddMicrosoftIdentityWebApi(options =>
    {
        builder.Configuration.Bind("AzureAdB2C", options);
    }, options => builder.Configuration.Bind("AzureAdB2C", options));

builder.Services.AddScoped<IClaimsTransformation, CustomClaimsTransformation>();
builder.Services.AddScoped<IShoppingListService, ShoppingListService.Data.ShoppingListService>();

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



