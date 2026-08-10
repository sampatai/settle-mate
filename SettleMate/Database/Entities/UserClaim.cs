using Microsoft.AspNetCore.Identity;

namespace SettleMate.Database.Entities;

public class UserClaim : IdentityUserClaim<string>
{
	public User User { get; set; }
}
