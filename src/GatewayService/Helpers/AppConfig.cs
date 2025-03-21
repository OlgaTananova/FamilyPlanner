using Microsoft.Identity.Web;

namespace GatewayService.Helpers;

public class AppConfig
{
    // Azure AD B2C
    public string AzureAdB2CInstance { get; set; } = string.Empty;
    public string AzureAdB2CClientId { get; set; } = string.Empty;
    public string AzureAdB2CDomain { get; set; } = string.Empty;
    public string AzureAdB2CSignUpSignInPolicyId { get; set; } = string.Empty;
    public string AzureAdB2CIssuer { get; set; } = string.Empty;
    public string AzureAdB2CTenantId { get; set; } = string.Empty;

    // Application Insights
    public string ApplicationInsightsConnectionString { get; set; } = string.Empty;

    // CORS Origins
    public string[] ClientApps { get; set; } = Array.Empty<string>();

    // Database Connection Strings
    public string DefaultConnectionString { get; set; } = string.Empty;

    public string RabbitMqUser { get; set; } = string.Empty;
    public string RabbitMqHost { get; set; } = string.Empty;
    public string RabbitMqPassword { get; set; } = string.Empty;

    public string ReverseProxy { get; set; } = string.Empty;

    // Method to load configuration based on environment
    public static AppConfig LoadConfiguration(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var config = new AppConfig();

        if (environment.IsProduction())
        {
            // Load from environment variables in production
            config.AzureAdB2CInstance = Environment.GetEnvironmentVariable("AZURE_AD_B2C_INSTANCE") ?? "";
            config.AzureAdB2CClientId = Environment.GetEnvironmentVariable("AZURE_AD_B2C_CLIENT_ID") ?? "";
            config.AzureAdB2CDomain = Environment.GetEnvironmentVariable("AZURE_AD_B2C_DOMAIN") ?? "";
            config.AzureAdB2CSignUpSignInPolicyId = Environment.GetEnvironmentVariable("AZURE_AD_B2C_SIGNUP_SIGNIN_POLICY") ?? "";
            config.AzureAdB2CIssuer = Environment.GetEnvironmentVariable("AZURE_AD_B2C_ISSUER") ?? "";
            config.AzureAdB2CTenantId = Environment.GetEnvironmentVariable("AZURE_AD_B2C_TENANT_ID") ?? "";

            config.ApplicationInsightsConnectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING") ?? "";

            var clientApps = Environment.GetEnvironmentVariable("CLIENT_APPS") ?? "";
            config.ClientApps = clientApps.Split(',', StringSplitOptions.RemoveEmptyEntries);
            config.RabbitMqUser = Environment.GetEnvironmentVariable("RABBIT_MQ_USER") ?? "";
            config.RabbitMqPassword = Environment.GetEnvironmentVariable("RABBIT_MQ_PASSWORD") ?? "";
            config.RabbitMqHost = Environment.GetEnvironmentVariable("RABBIT_MQ_HOST") ?? "";
        }
        else if (environment.IsDevelopment() || environment.IsStaging())
        {
            config.ApplicationInsightsConnectionString = configuration.GetValue<string>("ApplicationInsights:ConnectionString") ?? "";
            var clientApps = configuration.GetSection("ClientApps").Get<string[]>() ?? Array.Empty<string>();
            config.ClientApps = clientApps;

            config.DefaultConnectionString = configuration.GetConnectionString("DefaultConnection") ?? "";

            config.RabbitMqUser = configuration.GetValue<string>("RabbitMq:User") ?? "";
            config.RabbitMqPassword = configuration.GetValue<string>("RabbitMq:Password") ?? "";
            config.RabbitMqHost = configuration.GetValue<string>("RabbitMq:Host") ?? "";
        }

        return config;
    }

    public static void LoadAzureAdB2CConfig(MicrosoftIdentityOptions options, AppConfig appConfig, IConfiguration configuration, IWebHostEnvironment environment)
    {
        if (environment.IsProduction())
        {
            options.Instance = appConfig.AzureAdB2CInstance;
            options.ClientId = appConfig.AzureAdB2CClientId;
            options.Domain = appConfig.AzureAdB2CDomain;
            options.SignUpSignInPolicyId = appConfig.AzureAdB2CSignUpSignInPolicyId;
            options.TenantId = appConfig.AzureAdB2CTenantId;
        }
        else
        {
            configuration.Bind("AzureAdB2C", options);
        }
    }

}
