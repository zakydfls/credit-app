using CreditApplication.Domain.Entities;

namespace CreditApplication.Application.Abstractions.Authentication;

public enum PasswordCheckResult { Failed, Success, SuccessRehashNeeded }

public interface IPasswordService
{
    string Hash(User user, string password);
    PasswordCheckResult Verify(User user, string password);
}
