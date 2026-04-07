namespace ApplicationTracker.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    public ICollection<Company> Companies { get; private set; } = new List<Company>();
    public ICollection<JobApplication> JobApplications { get; private set; } = new List<JobApplication>();
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    private User() { }

    public static User Create(string email, string passwordHash, string name)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            Name = name,
            CreatedAt = DateTime.UtcNow
        };
    }
}
