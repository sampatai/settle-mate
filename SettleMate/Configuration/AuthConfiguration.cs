namespace SettleMate.Configuration
{
    public record AuthConfiguration
    {
        public required string Key { get; init; }
        public required string Issuer { get; init; }
        public required string Audience { get; init; }
        public int AccessTokenMinutes { get; set; }
        public int RefreshTokenDays { get; set; }
    }

}
