using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Configuration;
using SettleMate.Database;
using SettleMate.Features.Users.Shared;


namespace SettleMate.Features.Users.Login
{
    public class RefreshTokenHandler(ITokenHelper tokenHelper) : IHandler<RefreshTokenRequest, Result<RefreshTokenResponse>>
    {
        public async Task<Result<RefreshTokenResponse>> HandleAsync(RefreshTokenRequest command, CancellationToken cancellationToken)
        {
            var result = await tokenHelper.RefreshTokenAsync(command.Token, command.RefreshToken, cancellationToken);
            return result;
        }
    }
}