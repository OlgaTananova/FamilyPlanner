using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;

public class CustomClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = (ClaimsIdentity)principal.Identity;

        // Map 'objectidentifier' to 'userid'
        var objectId = principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
        if (objectId != null)
        {
            identity.AddClaim(new Claim("userid", objectId));
        }

        // Map 'emails' to 'email'
        var email = principal.FindFirst("emails")?.Value;
        if (email != null)
        {
            identity.AddClaim(new Claim("email", email));
        }

        // Map 'extension_Family' to 'family'
        var family = principal.FindFirst("extension_Family")?.Value;
        if (family != null)
        {
            identity.AddClaim(new Claim("family", family));
        }

        // Map 'extension_IsAdmin' to 'admin'
        var isAdmin = principal.FindFirst("extension_IsAdmin")?.Value;
        if (isAdmin != null)
        {
            identity.AddClaim(new Claim("admin", isAdmin));
        }

        return Task.FromResult(principal);
    }
}
