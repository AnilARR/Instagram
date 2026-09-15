namespace Instagram.Api.Models;

public class Story
{
    public required string Id { get; init; }
    public required string AuthorId { get; init; }
    public required string ImageUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Stories expire 24h after they are posted, like the real thing.</summary>
    public DateTimeOffset ExpiresAt => CreatedAt.AddHours(24);

    public bool Seen { get; set; }
}
