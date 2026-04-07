using ApplicationTracker.Domain.Entities;
using ApplicationTracker.Domain.Errors;
using ApplicationTracker.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;

    public RegisterCommandHandler(IUserRepository users, IPasswordHasher hasher)
    {
        _users = users;
        _hasher = hasher;
    }

    public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken ct)
    {
        if (await _users.ExistsByEmailAsync(request.Email, ct))
            return Result.Fail<RegisterResponse>(DomainErrors.Auth.EmailAlreadyTaken);

        var hash = _hasher.Hash(request.Password);
        var user = User.Create(request.Email, hash, request.Name);

        await _users.AddAsync(user, ct);
        await _users.SaveChangesAsync(ct);

        return Result.Ok(new RegisterResponse(user.Id, user.Email, user.Name));
    }
}
