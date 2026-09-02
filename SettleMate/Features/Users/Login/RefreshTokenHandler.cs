using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Configuration;
using SettleMate.Database;
using SettleMate.Database.Entities.Identity;
using SettleMate.Features.Users.Shared;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SettleMate.Features.Users.Login
{
    public class RefreshTokenHandler : IHandler<RefreshTokenRequest, Result<RefreshTokenResponse>>
    {
        private const string ErrorUserNotFound = "user_not_found";
        private const string ErrorInvalidToken = "invalid_token";
        private readonly ApplicationDbContext _dbContext;
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly AuthConfiguration _authConfiguration;

        public RefreshTokenHandler(ApplicationDbContext dbContext, 
            RoleManager<Role> roleManager,
            UserManager<User> userManager,
            TokenValidationParameters tokenValidationParameters,
            IOptions<AuthConfiguration> authOptions
             )
        {
            _dbContext = dbContext;
            _roleManager = roleManager;
            _userManager = userManager;
            _tokenValidationParameters = tokenValidationParameters;
            _authConfiguration = authOptions.Value;
        }
        public async Task<Result<RefreshTokenResponse>> HandleAsync(RefreshTokenRequest command, CancellationToken cancellationToken)
        {
            var validatedToken = GetPrincipalFromToken(command.Token, _tokenValidationParameters);
            if (validatedToken is null)
            {
                return Result.Failure<RefreshTokenResponse>(Error.Failure(ErrorInvalidToken, "Invalid token"));
            }

            var jti = validatedToken.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;
            if (string.IsNullOrEmpty(jti))
            {
                return Result.Failure<RefreshTokenResponse>(Error.Failure(ErrorInvalidToken, "Invalid token"));
            }

            var storedRefreshToken = await _dbContext.Set<RefreshToken>()
                .FirstOrDefaultAsync(x => x.Token == command.RefreshToken, cancellationToken);
            if (storedRefreshToken is null)
            {
                return Result.Failure<RefreshTokenResponse>(Error.Failure(ErrorInvalidToken, "This refresh token does not exist"));
            }

            if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
            {
                return Result.Failure<RefreshTokenResponse>(Error.Failure(ErrorInvalidToken, "This refresh token has expired"));
            }

            if (storedRefreshToken.Invalidated)
            {
                return Result.Failure<RefreshTokenResponse>(Error.Failure(ErrorInvalidToken, "This refresh token has been invalidated"));
            }

            if (storedRefreshToken.JwtId != jti)
            {
                return Result.Failure<RefreshTokenResponse>(Error.Failure(ErrorInvalidToken, "This refresh token does not match this JWT"));
            }

            var userId = storedRefreshToken.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                return Result.Failure<RefreshTokenResponse>(Error.NotFound(ErrorUserNotFound, "Current user is not found"));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return Result.Failure<RefreshTokenResponse>(Error.NotFound(ErrorUserNotFound, "Current user is not found"));
            }

            storedRefreshToken.Invalidated = true;
            var (newToken, newRefreshToken) = await GenerateJwtAndRefreshTokenAsync(user, cancellationToken);
            return Result<RefreshTokenResponse>.Success(new RefreshTokenResponse(newToken, newRefreshToken));
        }

        private async Task<(string Token, string RefreshToken)> GenerateJwtAndRefreshTokenAsync(
            User user,
            CancellationToken cancellationToken)
        {
            var jwtId = Guid.NewGuid().ToString();
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "user";
            var roleEntity = await _roleManager.FindByNameAsync(role);
            var roleClaims = roleEntity is null ? [] : await _roleManager.GetClaimsAsync(roleEntity);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email!),
                new(JwtRegisteredClaimNames.Jti, jwtId),
                new(JwtRegisteredClaimNames.Name, $"{user.FirstName} {user.LastName}"),
                new("userid", user.Id),
                new("role", role)
            };
            claims.AddRange(roleClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authConfiguration.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _authConfiguration.Issuer,
                audience: _authConfiguration.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_authConfiguration.AccessTokenMinutes),
                signingCredentials: credentials);

            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            _dbContext.Set<RefreshToken>().Add(new RefreshToken
            {
                Token = refreshToken,
                JwtId = jwtId,
                ExpiryDate = DateTime.UtcNow.AddDays(_authConfiguration.RefreshTokenDays),
                UserId = user.Id
            });
            await _dbContext.SaveChangesAsync(cancellationToken);

            return (new JwtSecurityTokenHandler().WriteToken(token), refreshToken);
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
