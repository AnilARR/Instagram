namespace Instagram.Api.Models;

public class Comment
{
    public required string Id { get; init; }
    public required string PostId { get; init; }
    public required string AuthorId { get; init; }
    public required string Text { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public HashSet<string> LikedByIds { get; } = new();
}
