using Instagram.Api.Data;
using Instagram.Api.Dtos;
using Instagram.Api.Models;

namespace Instagram.Api.Services;

/// <summary>
/// Projects the in-memory models onto the DTOs the Angular client consumes.
/// Anything "relative to me" (liked, followed) is resolved here, so the client
/// never has to compare id lists itself.
/// </summary>
public class DtoMapper(DummyDataStore store)
{
    /// <summary>Comments shown inline on a feed card before "view all".</summary>
    private const int InlineCommentCount = 2;

    public UserSummaryDto ToSummary(User user) =>
        new(user.Id, user.Username, user.FullName, user.AvatarUrl, user.IsVerified);

    public UserSummaryDto? ToSummaryById(string userId)
    {
        var user = store.FindUser(userId);
        return user is null ? null : ToSummary(user);
    }

    public UserProfileDto ToProfile(User user)
    {
        var me = store.CurrentUser;

        return new UserProfileDto(
            user.Id,
            user.Username,
            user.FullName,
            user.AvatarUrl,
            user.Bio,
            user.Website,
            user.IsVerified,
            store.CountPosts(user.Id),
            store.CountFollowers(user.Id),
            user.FollowingIds.Count,
            me.FollowingIds.Contains(user.Id),
            user.Id == me.Id);
    }

    public CommentDto ToDto(Comment comment)
    {
        var author = store.FindUser(comment.AuthorId);

        return new CommentDto(
            comment.Id,
            comment.PostId,
            author is null ? UnknownAuthor(comment.AuthorId) : ToSummary(author),
            comment.Text,
            comment.CreatedAt,
            comment.LikedByIds.Count,
            comment.LikedByIds.Contains(store.CurrentUserId));
    }

    /// <param name="allComments">
    /// When false only the newest few comments ride along, which is what the
    /// feed needs; the profile and post detail views ask for all of them.
    /// </param>
    public PostDto ToDto(Post post, bool allComments = false)
    {
        var author = store.FindUser(post.AuthorId);

        var ordered = post.Comments.OrderBy(c => c.CreatedAt).ToList();
        var visible = allComments
            ? ordered
            : ordered.TakeLast(InlineCommentCount).ToList();

        return new PostDto(
            post.Id,
            author is null ? UnknownAuthor(post.AuthorId) : ToSummary(author),
            post.ImageUrl,
            post.Caption,
            post.Location,
            post.CreatedAt,
            post.LikedByIds.Count,
            post.LikedByIds.Contains(store.CurrentUserId),
            post.Comments.Count,
            visible.Select(ToDto).ToList());
    }

    public StoryDto ToDto(Story story)
    {
        var author = store.FindUser(story.AuthorId);

        return new StoryDto(
            story.Id,
            author is null ? UnknownAuthor(story.AuthorId) : ToSummary(author),
            story.ImageUrl,
            story.CreatedAt,
            story.Seen);
    }

    /// <summary>
    /// Stand-in for an author that is no longer in the store, so a dangling id
    /// degrades to a placeholder instead of throwing mid-serialisation.
    /// </summary>
    private static UserSummaryDto UnknownAuthor(string userId) =>
        new(userId, "unknown", "Unknown user", "https://i.pravatar.cc/200?u=unknown", false);
}
