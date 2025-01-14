using System;
using System.Security.Claims;

namespace CatalogService.IntegrationTests.Utils;

public class AuthHelper
{
    public static Dictionary<string, object> GetBearerForUser(string userId, string family)
    {
        return new Dictionary<string, object> {
                {
                    "userId", userId

                },
                {
                    "family", family
                }
            };
    }
}
