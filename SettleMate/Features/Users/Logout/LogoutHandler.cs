using Microsoft.AspNetCore.Identity;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database.Entities.Identity;

namespace SettleMate.Features.Users.Logout;

public sealed class LogoutHandler(SignInManager<User> signInManager)
    : IHandler<Unit, Result<Unit>>
{
    public async Task<Result<Unit>> HandleAsync(
        Unit request,
        CancellationToken cancellationToken)
    {
        await signInManager.SignOutAsync();
        return Result<Unit>.Success(Unit.Value);
    }
}