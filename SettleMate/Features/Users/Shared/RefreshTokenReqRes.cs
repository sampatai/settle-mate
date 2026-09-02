namespace SettleMate.Features.Users.Shared
{
    public sealed record RefreshTokenRequest(string Token, string RefreshToken);
    public sealed record RefreshTokenResponse(string Token, string RefreshToken);

}
