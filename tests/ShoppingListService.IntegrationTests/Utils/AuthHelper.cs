using System;

namespace ShoppingListService.IntegrationTests.Utils;

public static class AuthHelper
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
