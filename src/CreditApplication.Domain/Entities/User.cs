namespace CreditApplication.Domain.Entities;

public sealed class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = null!;
    public string NormalizedUsername { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;

    private User() { }

    public User(string username)
    {
        Id = Guid.NewGuid();
        Username = username;
        NormalizedUsername = username.ToUpperInvariant();
    }

    public void SetPasswordHash(string passwordHash) => PasswordHash = passwordHash;
}
