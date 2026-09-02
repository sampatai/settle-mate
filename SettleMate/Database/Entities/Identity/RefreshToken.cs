using SettleMate.Abstractions;
using SettleMate.Database.Entities.Identity;

namespace SettleMate.Features.Users.Login;

public class RefreshToken : AuditableEntity
{
    public RefreshToken()
    {
			
    }
    public string Token { get; set; } = null!;

	public string JwtId { get; set; } = null!;

	public DateTime ExpiryDate { get; set; }

	public bool Invalidated { get; set; }

	public string UserId { get; set; } = null!;

	public User User { get; set; } = null!;

	
}
