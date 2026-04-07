using ApplicationTracker.Domain.Enums;

namespace ApplicationTracker.Domain.Entities;

public class JobApplication
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid CompanyId { get; private set; }
    public string JobTitle { get; private set; } = string.Empty;
    public ApplicationStatus Status { get; private set; }
    public string? JobUrl { get; private set; }
    public string? Location { get; private set; }
    public decimal? SalaryExpectation { get; private set; }
    public DateTime AppliedAt { get; private set; }
    public DateTime? NextActionAt { get; private set; }
    public string? NextActionNote { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public User User { get; private set; } = null!;
    public Company Company { get; private set; } = null!;
    public ICollection<ApplicationNote> Notes { get; private set; } = new List<ApplicationNote>();

    private JobApplication() { }

    public static JobApplication Create(
        Guid userId,
        Guid companyId,
        string jobTitle,
        ApplicationStatus status,
        string? jobUrl,
        string? location,
        decimal? salaryExpectation,
        DateTime appliedAt,
        DateTime? nextActionAt,
        string? nextActionNote)
    {
        return new JobApplication
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CompanyId = companyId,
            JobTitle = jobTitle,
            Status = status,
            JobUrl = jobUrl,
            Location = location,
            SalaryExpectation = salaryExpectation,
            AppliedAt = appliedAt,
            NextActionAt = nextActionAt,
            NextActionNote = nextActionNote,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string jobTitle,
        ApplicationStatus status,
        string? jobUrl,
        string? location,
        decimal? salaryExpectation,
        DateTime appliedAt,
        DateTime? nextActionAt,
        string? nextActionNote)
    {
        JobTitle = jobTitle;
        Status = status;
        JobUrl = jobUrl;
        Location = location;
        SalaryExpectation = salaryExpectation;
        AppliedAt = appliedAt;
        NextActionAt = nextActionAt;
        NextActionNote = nextActionNote;
    }
}
