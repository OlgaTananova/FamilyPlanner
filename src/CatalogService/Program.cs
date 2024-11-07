using CatalogService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add User Secrets Manager to the container
builder.Configuration.AddUserSecrets<Program>();

// Get the secrets from user secrets
var configuration = builder.Configuration;
var username = configuration["PostgresUser"];
var password = configuration["PostgresPassword"];
var database = configuration["Database"];

// Construct the connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    .Replace("{PostgresUser}", username)
    .Replace("{PostgresPassword}", password)
    .Replace("{Database}", database);

// Add services to the container.

Console.WriteLine(connectionString);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDbContext<CatalogDbContext>(opt =>
{
    opt.UseNpgsql(connectionString);
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ICatalogRepository, CatalogRepository>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
