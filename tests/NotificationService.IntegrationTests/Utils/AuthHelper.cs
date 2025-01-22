using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WebMotions.Fake.Authentication.JwtBearer;
namespace NotificationService.IntegrationTests.Utils;

public static class AuthHelper
{
    public static string GetBearerForUser(string userId, string family)
    {


        //Define token claims
        var claims = new List<Claim>
        {
            new Claim("userId", userId),
            new Claim("family", family),
            new Claim("given_name", "TestUser"),
            new Claim("role", "User")
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("YourSuperSecretKey1234567891011444422876");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);

    }
}
