using Contracts.Catalog;
using DotNetEnv;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using NotificationService.Consumers;
using NotificationService.Hubs;
using ShoppingListService.Consumers;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

var rabbitmqUser = Environment.GetEnvironmentVariable("RABBIT_MQ_USER");
var rabbitmqPassword = Environment.GetEnvironmentVariable("RABBIT_MQ_PASSWORD");

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

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


    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("notifications", false));
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.UseMessageRetry(r =>
        {
            r.Handle<RabbitMqConnectionException>();
            r.Interval(5, TimeSpan.FromSeconds(10));
        });
        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", h =>
        {
            h.Username(rabbitmqUser!);
            h.Password(rabbitmqPassword!);
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:3000"); // Allow only these origins
        policy.AllowAnyHeader();
        policy.WithMethods("GET", "POST");
        policy.AllowCredentials();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options =>
    {
        builder.Configuration.Bind("AzureAdB2C", options);
    }, options => builder.Configuration.Bind("AzureAdB2C", options));


builder.Services.AddScoped<IClaimsTransformation, CustomClaimsTransformation>();
builder.Services.AddAuthorization();

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.UseCors("AllowSpecificOrigins");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/notifications").RequireAuthorization();

app.Run();
