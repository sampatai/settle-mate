using Microsoft.AspNetCore.Identity;

namespace SettleMate.Database.Entities.Identity;

public class RoleClaim : IdentityRoleClaim<string>
{
	public Role Role { get; set; }
}
