namespace Instagram.Api.Dtos;

/// <summary>Compact author block embedded in posts, comments and stories.</summary>
public record UserSummaryDto(
    string Id,
    string Username,
    string FullName,
    string AvatarUrl,
    bool IsVerified);

public record UserProfileDto(
    string Id,
    string Username,
    string FullName,
    string AvatarUrl,
    string Bio,
    string Website,
    bool IsVerified,
    int PostCount,
    int FollowerCount,
    int FollowingCount,
    bool IsFollowedByCurrentUser,
    bool IsCurrentUser);

public record CommentDto(
    string Id,
    string PostId,
    UserSummaryDto Author,
    string Text,
    DateTimeOffset CreatedAt,
    int LikeCount,
    bool LikedByCurrentUser);

public record PostDto(
    string Id,
    UserSummaryDto Author,
    string ImageUrl,
    string Caption,
    string Location,
    DateTimeOffset CreatedAt,
    int LikeCount,
    bool LikedByCurrentUser,
    int CommentCount,
    IReadOnlyList<CommentDto> Comments);

public record StoryDto(
    string Id,
    UserSummaryDto Author,
    string ImageUrl,
    DateTimeOffset CreatedAt,
    bool Seen);

/// <summary>A page of results plus the cursor needed to fetch the next one.</summary>
public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public bool HasMore => Page * PageSize < TotalCount;
}

public record CreateCommentRequest(string Text);

public record CreatePostRequest(string ImageUrl, string? Caption, string? Location);

/// <summary>Result of a toggle action, so the client can reconcile its state.</summary>
public record LikeResultDto(string PostId, int LikeCount, bool LikedByCurrentUser);

public record FollowResultDto(string UserId, int FollowerCount, bool IsFollowedByCurrentUser);
