namespace ApplicationTracker.Domain.Entities;

public class Company
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Website { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public User User { get; private set; } = null!;
    public ICollection<JobApplication> JobApplications { get; private set; } = new List<JobApplication>();

    private Company() { }

    public static Company Create(Guid userId, string name, string? website, string? notes)
    {
        return new Company
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            Website = website,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? website, string? notes)
    {
        Name = name;
        Website = website;
        Notes = notes;
    }
}
