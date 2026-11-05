using CreditApplication.Application.Abstractions.Authentication;
using CreditApplication.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CreditApplication.Infrastructure.Authentication;

public sealed class PasswordService(IPasswordHasher<User> hasher) : IPasswordService
{
    public string Hash(User user, string password) => hasher.HashPassword(user, password);

    public PasswordCheckResult Verify(User user, string password) =>
        hasher.VerifyHashedPassword(user, user.PasswordHash, password) switch
        {
            PasswordVerificationResult.Success => PasswordCheckResult.Success,
            PasswordVerificationResult.SuccessRehashNeeded => PasswordCheckResult.SuccessRehashNeeded,
            _ => PasswordCheckResult.Failed
        };
}
