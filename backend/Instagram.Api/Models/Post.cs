namespace Instagram.Api.Models;

public class Post
{
    public required string Id { get; init; }
    public required string AuthorId { get; init; }
    public required string ImageUrl { get; set; }
    public string Caption { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Ids of the users who liked this post.</summary>
    public HashSet<string> LikedByIds { get; } = new();

    public List<Comment> Comments { get; } = new();
}
