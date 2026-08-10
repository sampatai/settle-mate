using Microsoft.AspNetCore.Identity;

namespace SettleMate.Database.Entities;

public class RoleClaim : IdentityRoleClaim<string>
{
	public Role Role { get; set; }
}
