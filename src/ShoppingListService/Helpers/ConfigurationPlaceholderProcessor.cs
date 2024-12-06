using System;
using System.Text.RegularExpressions;

namespace ShoppingListService.Helpers;

public static class ConfigurationPlaceholderProcessor
{
    public static IConfiguration ProcessPlaceholders(IConfiguration configuration)
    {
        var inMemoryConfig = new Dictionary<string, string>();

        foreach (var kvp in configuration.AsEnumerable())
        {
            if (kvp.Value != null)
            {
                var processedValue = ReplacePlaceholders(kvp.Value);
                inMemoryConfig[kvp.Key] = processedValue;
            }
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();
    }

    private static string ReplacePlaceholders(string value)
    {
        var regex = new Regex(@"\{(.*?)\}");
        return regex.Replace(value, match =>
        {
            var envVarName = match.Groups[1].Value;
            return Environment.GetEnvironmentVariable(envVarName) ?? match.Value;
        });
    }
}