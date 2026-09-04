using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SettleMate.Abstractions.Errors;
using SettleMate.Configuration;
using SettleMate.Database;
using SettleMate.Database.Entities.Identity;
using SettleMate.Features.Users.Shared;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SettleMate.Features.Users.Login
{
    public interface ITokenHelper
    {
        Task<Result<RefreshTokenResponse>> RefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default);
        Task<Result<LoginResponse>> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    }
    public class TokenHelper(ApplicationDbContext dbContext,
            RoleManager<Role> roleManager,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
    TokenValidationParameters tokenValidationParameters,
            IOptions<AuthConfiguration> authOptions) : ITokenHelper
    {
        private const string ErrorUserNotFound = "user_not_found";
        private const string ErrorInvalidToken = "invalid_token";
        private const string ErrorInvalidCredentials = "invalid_credentials";

        public async Task<Result<LoginResponse>> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                return Result<LoginResponse>.Failure(ErrorUserNotFound, "User not found");
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, password, false);
            if (!result.Succeeded)
            {
                return Result<LoginResponse>.Failure(ErrorInvalidCredentials, "Invalid credentials");
            }

            var (token, refreshToken) = await GenerateJwtAndRefreshTokenAsync(user, null);

            return Result<LoginResponse>.Success(new LoginResponse(token, refreshToken));
        }
        public async Task<Result<RefreshTokenResponse>> RefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken)
        {
            var validatedToken = GetPrincipalFromToken(token, tokenValidationParameters);
            if (validatedToken is null)
            {
                return Result<RefreshTokenResponse>.Failure(ErrorInvalidToken, "Invalid token");
            }

            var jti = validatedToken.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;
            if (string.IsNullOrEmpty(jti))
            {
                return Result<RefreshTokenResponse>.Failure(ErrorInvalidToken, "Invalid token");
            }

            var storedRefreshToken = await dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken, cancellationToken);
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

            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return Result<RefreshTokenResponse>.Failure(ErrorUserNotFound, "Current user is not found");
            }

            var (newToken, newRefreshToken) = await GenerateJwtAndRefreshTokenAsync(user, refreshToken);
            return Result<RefreshTokenResponse>.Success(new RefreshTokenResponse(newToken, newRefreshToken));
        }
        private async Task<(string token, string refreshToken)> GenerateJwtAndRefreshTokenAsync(User user, string? existingRefreshToken)
        {
            var roles = await userManager.GetRolesAsync(user);
            var userRole = roles.FirstOrDefault() ?? "user";

            var role = await roleManager.FindByNameAsync(userRole);
            var roleClaims = role is not null ? await roleManager.GetClaimsAsync(role) : [];

            var token = GenerateJwtToken(user, authOptions.Value, userRole, roleClaims);
            var refreshToken = await GenerateRefreshTokenAsync(token, user, existingRefreshToken);

            return (token, refreshToken);
        }

        private async Task<string> GenerateRefreshTokenAsync(string token, User user, string? existingRefreshToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var jti = jwtToken.Id;

            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                JwtId = jti,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
            };

            if (!string.IsNullOrEmpty(existingRefreshToken))
            {
                var existingToken = await dbContext.Set<RefreshToken>().FirstOrDefaultAsync(x => x.Token == existingRefreshToken);
                if (existingToken != null)
                {
                    dbContext.Set<RefreshToken>().Remove(existingToken);
                }
            }

            await dbContext.AddAsync(refreshToken);
            await dbContext.SaveChangesAsync();

            return refreshToken.Token;
        }

        private static string GenerateJwtToken(User user,
            AuthConfiguration authConfiguration,
            string userRole,
            IList<Claim> roleClaims)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authConfiguration.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenId = Guid.NewGuid().ToString();
            List<Claim> claims = [
                new(JwtRegisteredClaimNames.Sub, user.Email!),
            new("userid", user.Id),
            new("role", userRole),
            new(JwtRegisteredClaimNames.Jti, tokenId)
            ];

            foreach (var roleClaim in roleClaims)
            {
                claims.Add(new Claim(roleClaim.Type, roleClaim.Value));
            }

            var token = new JwtSecurityToken(
                issuer: authConfiguration.Issuer,
                audience: authConfiguration.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
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

