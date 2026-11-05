using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CreditApplication.Application.Abstractions.Authentication;
using CreditApplication.Application.Auth;
using CreditApplication.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CreditApplication.Infrastructure.Authentication;

public sealed class JwtTokenService(IOptions<JwtOptions> options) : ITokenService
{
    public AuthenticationResult CreateToken(User user)
    {
        var jwt = options.Value;
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(15);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(jwt.Issuer, jwt.Audience, claims,
            notBefore: now.UtcDateTime, expires: expiresAt.UtcDateTime, signingCredentials: credentials);

        return new AuthenticationResult(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
