using Microsoft.AspNetCore.Identity;

namespace SettleMate.Database.Entities.Identity;

public class UserClaim : IdentityUserClaim<string>
{
	public User User { get; set; }
}
