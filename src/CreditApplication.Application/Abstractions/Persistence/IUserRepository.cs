using CreditApplication.Domain.Entities;

namespace CreditApplication.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<User?> FindByNormalizedUsernameAsync(string normalizedUsername, CancellationToken cancellationToken);
    Task<bool> AddAsync(User user, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
