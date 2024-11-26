using CatalogService.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;

var builder = WebApplication.CreateBuilder(args);

IdentityModelEventSource.ShowPII = true;

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
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Allow only these origins
              .AllowAnyHeader()            // Allow any headers
              .AllowAnyMethod();           // Allow any HTTP methods
    });
});

// Configure authentication with Azure AD B2C
builder.Services.AddAuthentication("Bearer")
    .AddMicrosoftIdentityWebApi(options =>
    {
        builder.Configuration.Bind("AzureAdB2C", options);
        options.TokenValidationParameters.NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname";
        options.TokenValidationParameters.RoleClaimType = "extension_Role";
    },
    options => builder.Configuration.Bind("AzureAdB2C", options));

// Add authorization
builder.Services.AddAuthorization();
builder.Services.AddScoped<IClaimsTransformation, CustomClaimsTransformation>();




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Enable CORS
app.UseCors("AllowSpecificOrigins");

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
