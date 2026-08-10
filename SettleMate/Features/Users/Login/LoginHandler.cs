using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Configuration;
using SettleMate.Database.Entities.Identity;
using SettleMate.Features.Users.Shared;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SettleMate.Features.Users.Login
{
    public class LoginHandler(
        IOptions<AuthConfiguration> authOptions,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        RoleManager<Role> roleManager,
        IEventDispatcher events) : IHandler<LoginUserRequest, Result<UserResponse>>
    {
        public async Task<Result<UserResponse>> HandleAsync(LoginUserRequest request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return Result.Failure<UserResponse>(Error.NotFound("user.NotFound", "User not found"));
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
            {
                return Result.Failure<UserResponse>(Error.Unauthorized("user.InvalidCredentials", "Invalid credentials"));
            }

            var roles = await userManager.GetRolesAsync(user);
            var userRole = roles.FirstOrDefault() ?? "user";

            var role = await roleManager.FindByNameAsync(userRole);
            var roleClaims = role is not null ? await roleManager.GetClaimsAsync(role) : [];

            return Result.Success(new UserResponse(GenerateJwtToken(user, authOptions.Value, userRole, roleClaims)));
        }

        private static string GenerateJwtToken(User user,
            AuthConfiguration authConfiguration,
            string userRole,
            IList<Claim> roleClaims)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authConfiguration.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            List<Claim> claims = [
                new(JwtRegisteredClaimNames.Email, user.Email!),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Name, $"{user.FirstName} {user.LastName}"),
                new(JwtRegisteredClaimNames.Email, user.Email!),
            new("role", userRole),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())

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
    }




}