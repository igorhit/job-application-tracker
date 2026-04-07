using FluentResults;

namespace ApplicationTracker.Domain.Errors;

public static class DomainErrors
{
    public static class Auth
    {
        public static IError EmailAlreadyTaken => new Error("Email already taken");
        public static IError InvalidCredentials => new Error("Invalid credentials");
        public static IError InvalidRefreshToken => new Error("Invalid or expired refresh token");
        public static IError Unauthorized => new Error("Unauthorized");
    }

    public static class Company
    {
        public static IError NotFound => new Error("Company not found");
    }

    public static class JobApplication
    {
        public static IError NotFound => new Error("Job application not found");
        public static IError CompanyNotFound => new Error("Company not found for this user");
    }

    public static class Note
    {
        public static IError NotFound => new Error("Note not found");
        public static IError ApplicationNotFound => new Error("Job application not found for this user");
    }
}
