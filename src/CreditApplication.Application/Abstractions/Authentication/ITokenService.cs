using CreditApplication.Application.Auth;
using CreditApplication.Domain.Entities;

namespace CreditApplication.Application.Abstractions.Authentication;

public interface ITokenService
{
    AuthenticationResult CreateToken(User user);
}
