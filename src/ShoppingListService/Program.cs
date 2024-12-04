using Contracts.Catalog;
using DotNetEnv;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ShoppingListService.Consumers;
using ShoppingListService.Data;
using ShoppingListService.Helpers;

var builder = WebApplication.CreateBuilder(args);

Env.Load();


// Add configuration sources
var username = Environment.GetEnvironmentVariable("POSTGRES_USER");
var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
var database = Environment.GetEnvironmentVariable("POSTGRES_DATABASE");
var adIstance = Environment.GetEnvironmentVariable("AZURE_AD_B2C_INSTANCE");
var clientId = Environment.GetEnvironmentVariable("AZURE_AD_B2C_CLIENT_ID");
var domain = Environment.GetEnvironmentVariable("AZURE_AD_B2C_DOMAIN");
var policy = Environment.GetEnvironmentVariable("AZURE_AD_B2C_SIGN_UP_SIGN_IN_POLICY_ID");
var issuer = Environment.GetEnvironmentVariable("AZURE_AD_B2C_ISSUER");
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

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

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



