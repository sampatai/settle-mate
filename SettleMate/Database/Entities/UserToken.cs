using Microsoft.AspNetCore.Identity;

namespace SettleMate.Database.Entities;

public class UserToken : IdentityUserToken<string>
{
	public User User { get; set; }
}
