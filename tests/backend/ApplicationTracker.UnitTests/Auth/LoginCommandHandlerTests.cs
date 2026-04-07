using ApplicationTracker.Application.Features.Auth.Commands.Login;
using ApplicationTracker.Domain.Entities;
using ApplicationTracker.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace ApplicationTracker.UnitTests.Auth;

public class LoginCommandHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtService _jwt = Substitute.For<IJwtService>();

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsTokens()
    {
        var user = User.Create("user@test.com", "hashed", "Test User");
        _users.GetByEmailAsync("user@test.com", Arg.Any<CancellationToken>()).Returns(user);
        _hasher.Verify("Password123!", "hashed").Returns(true);
        _jwt.GenerateAccessToken(user).Returns("access-token");
        _jwt.GenerateRefreshToken().Returns("refresh-token");

        var handler = new LoginCommandHandler(_users, _refreshTokens, _hasher, _jwt);
        var result = await handler.Handle(new LoginCommand("user@test.com", "Password123!"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access-token");
        result.Value.RefreshToken.Should().Be("refresh-token");
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsFail()
    {
        _users.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var handler = new LoginCommandHandler(_users, _refreshTokens, _hasher, _jwt);
        var result = await handler.Handle(new LoginCommand("notfound@test.com", "Password123!"), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WrongPassword_ReturnsFail()
    {
        var user = User.Create("user@test.com", "hashed", "Test User");
        _users.GetByEmailAsync("user@test.com", Arg.Any<CancellationToken>()).Returns(user);
        _hasher.Verify("WrongPassword", "hashed").Returns(false);

        var handler = new LoginCommandHandler(_users, _refreshTokens, _hasher, _jwt);
        var result = await handler.Handle(new LoginCommand("user@test.com", "WrongPassword"), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }
}
