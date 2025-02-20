using System.Security.Claims;

namespace Contracts.Authentication;

public static class ClaimsPrincipalExtension
{
    public static string GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal?.FindFirst("userId")?.Value;
    }

    public static string GetFamilyName(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal?.FindFirst("family")?.Value;
    }

}
