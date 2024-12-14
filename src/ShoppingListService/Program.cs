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

Env.Load();

var rabbitmqUser = Environment.GetEnvironmentVariable("RABBIT_MQ_USER");
var rabbitmqPassword = Environment.GetEnvironmentVariable("RABBIT_MQ_PASSWORD");


// Add configuration sources
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// Process placeholders in configuration
var processedConfiguration = ConfigurationPlaceholderProcessor.ProcessPlaceholders(builder.Configuration);

// Use processedConfiguration for your application
var connectionString = processedConfiguration.GetConnectionString("DefaultConnection");

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<ShoppingListContext>(opt =>
{
    opt.UseNpgsql(connectionString);
});
builder.Services.AddMassTransit(x =>
{
    x.AddConsumersFromNamespaceContaining<CatalogItemCreatedConsumer>();
    x.AddConsumersFromNamespaceContaining<CatalogItemUpdatedConsumer>();
    x.AddConsumersFromNamespaceContaining<CatalogCategoryUpdatedConsumer>();
    x.AddConsumersFromNamespaceContaining<CatalogItemDeletedConsumer>();

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
            h.Username(rabbitmqUser);
            h.Password(rabbitmqPassword);
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

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Allow only these origins
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
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowSpecificOrigins");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    DbInitializer.InitDb(app);
}
catch (Exception e)
{
    Console.WriteLine($"There is an error at db initialization. Message: {e.Message}");
}
app.Run();



