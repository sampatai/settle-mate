using Microsoft.AspNetCore.Identity;

namespace SettleMate.Database.Entities.Identity;

public class Role : IdentityRole
{
	public ICollection<UserRole> UserRoles { get; set; }
	public ICollection<RoleClaim> RoleClaims { get; set; }
}
