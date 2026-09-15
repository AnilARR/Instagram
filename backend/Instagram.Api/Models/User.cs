namespace Instagram.Api.Models;

/// <summary>
/// A person on the platform. Passwords are deliberately absent: this is a
/// dummy data set with no authentication.
/// </summary>
public class User
{
    public required string Id { get; init; }
    public required string Username { get; init; }
    public required string FullName { get; set; }
    public required string AvatarUrl { get; set; }
    public string Bio { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public bool IsVerified { get; set; }

    /// <summary>Ids of the users this user follows.</summary>
    public HashSet<string> FollowingIds { get; } = new();
}
