using CreditApplication.Application.Abstractions.Authentication;
using CreditApplication.Application.Abstractions.Persistence;
using CreditApplication.Application.Auth;
using CreditApplication.Domain.Entities;

namespace CreditApplication.UnitTests.Authentication;

public sealed class AuthenticationServiceTests
{
    [Fact]
    public async Task Authenticate_WhenRehashNeeded_SavesNewHashBeforeIssuingToken()
    {
        var user = new User("Alice");
        user.SetPasswordHash("old-hash");
        var users = new FakeUsers { ExistingUser = user };
        var tokens = new FakeTokens();
        var service = new AuthenticationService(users, new FakePasswords(PasswordCheckResult.SuccessRehashNeeded), tokens);

        var result = await service.AuthenticateAsync(new AuthenticateUserCommand("alice", "password123"), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("ALICE", users.LastLookup);
        Assert.Equal("new-hash", user.PasswordHash);
        Assert.True(users.Saved);
        Assert.Equal(user.Id, tokens.UserId);
    }

    [Fact]
    public async Task Register_WhenConcurrentDuplicateIsRejected_ReturnsNull()
    {
        var users = new FakeUsers { AddResult = false };
        var service = new AuthenticationService(users, new FakePasswords(PasswordCheckResult.Success), new FakeTokens());

        var result = await service.RegisterAsync(new RegisterUserCommand("Alice", "password123"), CancellationToken.None);

        Assert.Null(result);
        Assert.Equal("ALICE", users.LastLookup);
        Assert.Equal("new-hash", users.AddedUser?.PasswordHash);
    }

    private sealed class FakeUsers : IUserRepository
    {
        public User? ExistingUser { get; init; }
        public User? AddedUser { get; private set; }
        public string? LastLookup { get; private set; }
        public bool Saved { get; private set; }
        public bool AddResult { get; init; } = true;

        public Task<User?> FindByNormalizedUsernameAsync(string normalizedUsername, CancellationToken cancellationToken)
        {
            LastLookup = normalizedUsername;
            return Task.FromResult(ExistingUser);
        }

        public Task<bool> AddAsync(User user, CancellationToken cancellationToken)
        {
            AddedUser = user;
            return Task.FromResult(AddResult);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            Saved = true;
            return Task.CompletedTask;
        }
    }

    private sealed class FakePasswords(PasswordCheckResult result) : IPasswordService
    {
        public string Hash(User user, string password) => "new-hash";
        public PasswordCheckResult Verify(User user, string password) => result;
    }

    private sealed class FakeTokens : ITokenService
    {
        public Guid? UserId { get; private set; }
        public AuthenticationResult CreateToken(User user)
        {
            UserId = user.Id;
            return new AuthenticationResult("token", DateTimeOffset.UtcNow.AddMinutes(15));
        }
    }
}
