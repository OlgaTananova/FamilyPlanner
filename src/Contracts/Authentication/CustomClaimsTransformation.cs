using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

public class CustomClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = (ClaimsIdentity)principal.Identity;

        if (identity == null || !identity.IsAuthenticated)
        {
            return Task.FromResult(principal);
        }

        // Map 'objectidentifier' to 'userid'
        var objectId = principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
        if (objectId != null)
        {
            identity.AddClaim(new Claim("userId", objectId));
        }
        // Map 'givenname' to 'name'
        var givenName = principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname")?.Value;
        if (givenName != null)
        {
            identity.AddClaim(new Claim("givenName", givenName));
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
        // map extension_Role to roles
        var role = principal.FindFirst("extension_Role")?.Value;
        if (family != null)
        {
            identity.AddClaim(new Claim("role", role));
        }

        // Map 'extension_IsAdmin' to 'admin'
        var isAdmin = principal.FindFirst("extension_IsAdmin")?.Value;
        if (isAdmin != null)
        {
            identity.AddClaim(new Claim("admin", isAdmin));
        }
        // Map scp to scopes
        var scopes = principal.FindFirst("http://schemas.microsoft.com/identity/claims/scope")?.Value;
        if (scopes != null)
        {
            identity.AddClaim(new Claim("scopes", scopes));
        }
        // Check if the claims are present in the token
        if (string.IsNullOrEmpty(family) || string.IsNullOrEmpty(objectId))
        {
            throw new UnauthorizedAccessException("Missing required claims: family or userId.");
        }

        return Task.FromResult(principal);
    }
}
