using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SettleMate.Abstractions;
using SettleMate.Database;
using SettleMate.Database.Entities.Identity;
using SettleMate.Features.Users.Shared;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SettleMate.Features.Users.Login
{
    public class RefreshTokenHandler : IHandler<RefreshTokenRequest, Result<RefreshTokenResponse>>
    {
        private const string ErrorInvalidCredentials = "invalid_credentials";
        private const string ErrorUserNotFound = "user_not_found";
        private const string ErrorInvalidToken = "invalid_token";
        private const string ErrorInvalidInput = "invalid_input";
        private readonly ApplicationDbContext _dbContext;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public RefreshTokenHandler(ApplicationDbContext dbContext, 
            SignInManager<User> signInManager, 
            RoleManager<Role> roleManager ,
            UserManager<User> userManager,
            TokenValidationParameters tokenValidationParameters
             )
        {
            _dbContext = dbContext;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _userManager = userManager;
            _tokenValidationParameters = tokenValidationParameters;
        }
        public async Task<Result<RefreshTokenResponse>> HandleAsync(RefreshTokenRequest command, CancellationToken cancellationToken)
        {
            var validatedToken = GetPrincipalFromToken(command.Token, _tokenValidationParameters);
            if (validatedToken is null)
            {
                return Result<RefreshTokenResponse>.Failure(ErrorInvalidToken, "Invalid token");
            }

            var jti = validatedToken.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;
            if (string.IsNullOrEmpty(jti))
            {
                return Result<RefreshTokenResponse>.Failure(ErrorInvalidToken, "Invalid token");
            }

            var storedRefreshToken = await _dbContext.Set<RefreshToken>().FirstOrDefaultAsync(x => x.Token == refreshToken, cancellationToken);
            if (storedRefreshToken is null)
            {
                return Result<RefreshTokenResponse>.Failure(ErrorInvalidToken, "This refresh token does not exist");
            }

            if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
            {
                return Result<RefreshTokenResponse>.Failure(ErrorInvalidToken, "This refresh token has expired");
            }

            if (storedRefreshToken.Invalidated)
            {
                return Result<RefreshTokenResponse>.Failure(ErrorInvalidToken, "This refresh token has been invalidated");
            }

            if (storedRefreshToken.JwtId != jti)
            {
                return Result<RefreshTokenResponse>.Failure(ErrorInvalidToken, "This refresh token does not match this JWT");
            }

            var userId = validatedToken.Claims.FirstOrDefault(x => x.Type == "userid")?.Value;
            if (userId is null)
            {
                return Result<RefreshTokenResponse>.Failure(ErrorUserNotFound, "Current user is not found");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return Result<RefreshTokenResponse>.Failure(ErrorUserNotFound, "Current user is not found");
            }

            var (newToken, newRefreshToken) = await GenerateJwtAndRefreshTokenAsync(user, refreshToken);
            return Result<RefreshTokenResponse>.Success(new RefreshTokenResponse(newToken, newRefreshToken));
        }

        private static ClaimsPrincipal? GetPrincipalFromToken(string token, TokenValidationParameters parameters)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var tokenValidationParameters = parameters.Clone();
                tokenValidationParameters.ValidateLifetime = false;
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
                return IsJwtWithValidSecurityAlgorithm(validatedToken) ? principal : null;
            }
            catch
            {
                return null;
            }
        }

        private static bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
            => validatedToken is JwtSecurityToken jwtSecurityToken
               && jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

    }

}
