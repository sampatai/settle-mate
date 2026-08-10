using Microsoft.AspNetCore.Identity;

namespace SettleMate.Database.Entities;

public class UserRole : IdentityUserRole<string>
{
	public User User { get; set; }
	public Role Role { get; set; }
}
