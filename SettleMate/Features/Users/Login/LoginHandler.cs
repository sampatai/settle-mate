using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Features.Users.Shared;

namespace SettleMate.Features.Users.Login
{
    public class LoginHandler(
       ITokenHelper tokenHelper) : IHandler<LoginUserRequest, Result<LoginResponse>>
    {
        public async Task<Result<LoginResponse>> HandleAsync(LoginUserRequest request, CancellationToken cancellationToken)
        {
            return await tokenHelper.LoginAsync(request.Email, request.Password, cancellationToken);
        }
        
    }




}