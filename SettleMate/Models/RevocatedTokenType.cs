namespace SettleMate.Models;

public enum RevocatedTokenType
{
    /// <summary>
    /// Token is invalidated due to security concerns or user actions
    /// </summary>
    Invalidated,

    /// <summary>
    /// Token is invalidated due to role change
    /// </summary>
    RoleChanged
}