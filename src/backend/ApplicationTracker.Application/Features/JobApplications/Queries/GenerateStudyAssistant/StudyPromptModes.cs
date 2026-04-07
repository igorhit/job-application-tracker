namespace ApplicationTracker.Application.Features.JobApplications.Queries.GenerateStudyAssistant;

public static class StudyPromptModes
{
    public const string QuickStudy = "quick-study";
    public const string InterviewPrep = "interview-prep";
    public const string ReviewPlan = "review-plan";

    private static readonly string[] AllModes = [QuickStudy, InterviewPrep, ReviewPlan];

    public static bool IsValid(string mode)
    {
        return AllModes.Any(allowedMode => string.Equals(allowedMode, mode, StringComparison.OrdinalIgnoreCase));
    }

    public static string Normalize(string mode)
    {
        return AllModes.First(allowedMode => string.Equals(allowedMode, mode, StringComparison.OrdinalIgnoreCase));
    }
}
