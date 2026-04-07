namespace ApplicationTracker.Domain.Entities;

public class ApplicationNote
{
    public Guid Id { get; private set; }
    public Guid JobApplicationId { get; private set; }
    public Guid UserId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    public JobApplication JobApplication { get; private set; } = null!;

    private ApplicationNote() { }

    public static ApplicationNote Create(Guid jobApplicationId, Guid userId, string content)
    {
        return new ApplicationNote
        {
            Id = Guid.NewGuid(),
            JobApplicationId = jobApplicationId,
            UserId = userId,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string content)
    {
        Content = content;
    }
}
