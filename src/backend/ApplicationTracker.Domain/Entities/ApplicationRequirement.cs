namespace ApplicationTracker.Domain.Entities;

public class ApplicationRequirement
{
    public Guid Id { get; private set; }
    public Guid JobApplicationId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }

    public JobApplication JobApplication { get; private set; } = null!;

    private ApplicationRequirement() { }

    public static ApplicationRequirement Create(Guid jobApplicationId, string content, int displayOrder)
    {
        return new ApplicationRequirement
        {
            Id = Guid.NewGuid(),
            JobApplicationId = jobApplicationId,
            Content = content.Trim(),
            DisplayOrder = displayOrder,
        };
    }
}
