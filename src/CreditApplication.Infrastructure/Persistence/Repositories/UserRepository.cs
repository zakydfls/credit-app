using CreditApplication.Application.Abstractions.Persistence;
using CreditApplication.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CreditApplication.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(CreditApplicationDbContext dbContext) : IUserRepository
{
    public Task<User?> FindByNormalizedUsernameAsync(string normalizedUsername, CancellationToken cancellationToken) =>
        dbContext.Users.SingleOrDefaultAsync(user => user.NormalizedUsername == normalizedUsername, cancellationToken);

    public async Task<bool> AddAsync(User user, CancellationToken cancellationToken)
    {
        dbContext.Users.Add(user);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
               { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "ux_users_normalized_username" })
        {
            dbContext.Entry(user).State = EntityState.Detached;
            return false;
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
