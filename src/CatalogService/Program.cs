using CatalogService.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
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
    },
    options => builder.Configuration.Bind("AzureAdB2C", options));

builder.Services.AddScoped<IClaimsTransformation, CustomClaimsTransformation>();
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
    // Read
}
);



var app = builder.Build();
var policyProvider = app.Services.GetService<IAuthorizationPolicyProvider>();
Console.WriteLine($"Policies registered: {policyProvider.GetType().Name}");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Enable CORS
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
