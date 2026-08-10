using Microsoft.AspNetCore.Identity;

namespace SettleMate.Database.Entities.Identity;

public class UserLogin : IdentityUserLogin<string>
{
	public User User { get; set; }
}
