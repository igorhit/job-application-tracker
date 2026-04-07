using ApplicationTracker.Application.Features.Auth.Commands.Register;
using ApplicationTracker.Domain.Entities;
using ApplicationTracker.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace ApplicationTracker.UnitTests.Auth;

public class RegisterCommandHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();

    [Fact]
    public async Task Handle_NewEmail_ReturnsSuccess()
    {
        _users.ExistsByEmailAsync("new@test.com", Arg.Any<CancellationToken>()).Returns(false);
        _hasher.Hash("Password123!").Returns("hashed");

        var handler = new RegisterCommandHandler(_users, _hasher);
        var result = await handler.Handle(new RegisterCommand("new@test.com", "Password123!", "Test"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("new@test.com");
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsFail()
    {
        _users.ExistsByEmailAsync("dup@test.com", Arg.Any<CancellationToken>()).Returns(true);

        var handler = new RegisterCommandHandler(_users, _hasher);
        var result = await handler.Handle(new RegisterCommand("dup@test.com", "Password123!", "Test"), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }
}
