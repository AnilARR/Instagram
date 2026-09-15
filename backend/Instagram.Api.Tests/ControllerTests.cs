using Instagram.Api.Controllers;
using Instagram.Api.Data;
using Instagram.Api.Dtos;
using Instagram.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Instagram.Api.Tests;

/// <summary>
/// The controllers hold the toggle logic, so they are exercised directly
/// against a fresh store rather than over HTTP.
/// </summary>
public class ControllerTests
{
    private static (PostsController Posts, UsersController Users, StoriesController Stories, DummyDataStore Store) Build()
    {
        var store = new DummyDataStore();
        var mapper = new DtoMapper(store);

        return (
            new PostsController(store, mapper),
            new UsersController(store, mapper),
            new StoriesController(store, mapper),
            store);
    }

    private static T Body<T>(ActionResult<T> result)
    {
        // A successful action returns its payload either directly or wrapped in an OkObjectResult.
        if (result.Value is not null)
        {
            return result.Value;
        }

        var ok = Assert.IsType<OkObjectResult>(result.Result);

        // IsAssignableFrom, not IsType: an action declared as IReadOnlyList<T>
        // hands back a concrete List<T>.
        return Assert.IsAssignableFrom<T>(ok.Value);
    }

    [Fact]
    public void Feed_paginates_and_reports_whether_more_remain()
    {
        var (posts, _, _, _) = Build();

        var first = Body(posts.GetFeed(page: 1, pageSize: 2));

        Assert.Equal(2, first.Items.Count);
        Assert.Equal(1, first.Page);
        Assert.True(first.HasMore);

        var second = Body(posts.GetFeed(page: 2, pageSize: 2));

        // Page 2 must not repeat page 1.
        Assert.Empty(first.Items.Select(p => p.Id).Intersect(second.Items.Select(p => p.Id)));
    }

    [Fact]
    public void Feed_is_ordered_newest_first()
    {
        var (posts, _, _, _) = Build();

        var feed = Body(posts.GetFeed(page: 1, pageSize: 50));
        var dates = feed.Items.Select(p => p.CreatedAt).ToList();

        Assert.Equal(dates.OrderByDescending(d => d), dates);
    }

    [Fact]
    public void Feed_only_contains_posts_the_current_user_follows_or_owns()
    {
        var (posts, _, _, store) = Build();

        var feed = Body(posts.GetFeed(page: 1, pageSize: 50));
        var allowed = new HashSet<string>(store.CurrentUser.FollowingIds) { store.CurrentUserId };

        Assert.All(feed.Items, post => Assert.Contains(post.Author.Id, allowed));
    }

    [Fact]
    public void Feed_clamps_a_nonsense_page_size()
    {
        var (posts, _, _, _) = Build();

        Assert.Equal(1, Body(posts.GetFeed(page: 1, pageSize: 0)).PageSize);
        Assert.Equal(50, Body(posts.GetFeed(page: 1, pageSize: 999)).PageSize);
        // A page below 1 is treated as the first page rather than skipping backwards.
        Assert.Equal(1, Body(posts.GetFeed(page: -5, pageSize: 5)).Page);
    }

    [Fact]
    public void Liking_twice_returns_to_the_original_count()
    {
        var (posts, _, _, _) = Build();

        var original = Body(posts.GetPost("p1"));

        var liked = Body(posts.ToggleLike("p1"));
        Assert.NotEqual(original.LikedByCurrentUser, liked.LikedByCurrentUser);

        var unliked = Body(posts.ToggleLike("p1"));
        Assert.Equal(original.LikedByCurrentUser, unliked.LikedByCurrentUser);
        Assert.Equal(original.LikeCount, unliked.LikeCount);
    }

    [Fact]
    public void Liking_an_unknown_post_is_a_404()
    {
        var (posts, _, _, _) = Build();

        Assert.IsType<NotFoundResult>(posts.ToggleLike("no-such-post").Result);
    }

    [Fact]
    public void Adding_a_comment_attributes_it_to_the_current_user()
    {
        var (posts, _, _, store) = Build();

        var result = posts.AddComment("p1", new CreateCommentRequest("  hello  "));
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var comment = Assert.IsType<CommentDto>(created.Value);

        Assert.Equal(store.CurrentUserId, comment.Author.Id);
        // Whitespace is trimmed before it is stored.
        Assert.Equal("hello", comment.Text);
        Assert.Contains(comment.Id, Body(posts.GetComments("p1")).Select(c => c.Id));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void An_empty_comment_is_rejected(string text)
    {
        var (posts, _, _, _) = Build();

        var result = posts.AddComment("p1", new CreateCommentRequest(text));

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void A_comment_by_someone_else_cannot_be_deleted()
    {
        var (posts, _, _, store) = Build();

        var post = store.Posts.First(p =>
            p.Comments.Any(c => c.AuthorId != store.CurrentUserId));
        var theirComment = post.Comments.First(c => c.AuthorId != store.CurrentUserId);

        Assert.IsType<ForbidResult>(posts.DeleteComment(post.Id, theirComment.Id));
    }

    [Fact]
    public void Own_comment_can_be_deleted()
    {
        var (posts, _, _, _) = Build();

        var created = Assert.IsType<CreatedAtActionResult>(
            posts.AddComment("p1", new CreateCommentRequest("mine")).Result);
        var comment = Assert.IsType<CommentDto>(created.Value);

        Assert.IsType<NoContentResult>(posts.DeleteComment("p1", comment.Id));
        Assert.DoesNotContain(comment.Id, Body(posts.GetComments("p1")).Select(c => c.Id));
    }

    [Fact]
    public void Creating_a_post_requires_an_image_and_shows_up_on_the_profile()
    {
        var (posts, users, _, store) = Build();

        Assert.IsType<BadRequestObjectResult>(
            posts.CreatePost(new CreatePostRequest("  ", "caption", null)).Result);

        var before = Body(users.GetUserPosts(store.CurrentUser.Username)).Count;

        var created = Assert.IsType<CreatedAtActionResult>(
            posts.CreatePost(new CreatePostRequest("https://example.test/a.jpg", "hi", "Here")).Result);
        var post = Assert.IsType<PostDto>(created.Value);

        Assert.Equal(store.CurrentUserId, post.Author.Id);
        Assert.Equal(before + 1, Body(users.GetUserPosts(store.CurrentUser.Username)).Count);
    }

    [Fact]
    public void Profile_lookup_is_case_insensitive_and_404s_when_unknown()
    {
        var (_, users, _, store) = Build();

        var username = store.CurrentUser.Username;
        Assert.Equal(username, Body(users.GetProfile(username.ToUpperInvariant())).Username);

        Assert.IsType<NotFoundResult>(users.GetProfile("nobody-here").Result);
    }

    [Fact]
    public void Following_toggles_and_moves_the_follower_count()
    {
        var (_, users, _, store) = Build();

        var target = store.Users.First(u =>
            u.Id != store.CurrentUserId && store.CurrentUser.FollowingIds.Contains(u.Id));
        var before = store.CountFollowers(target.Id);

        var unfollowed = Body(users.ToggleFollow(target.Username));
        Assert.False(unfollowed.IsFollowedByCurrentUser);
        Assert.Equal(before - 1, unfollowed.FollowerCount);

        var refollowed = Body(users.ToggleFollow(target.Username));
        Assert.True(refollowed.IsFollowedByCurrentUser);
        Assert.Equal(before, refollowed.FollowerCount);
    }

    [Fact]
    public void You_cannot_follow_yourself()
    {
        var (_, users, _, store) = Build();

        var result = users.ToggleFollow(store.CurrentUser.Username);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void Suggestions_exclude_yourself_and_anyone_you_already_follow()
    {
        var (_, users, _, store) = Build();

        var suggestions = Body(users.GetSuggestions(limit: 10));

        Assert.DoesNotContain(store.CurrentUserId, suggestions.Select(s => s.Id));
        Assert.Empty(suggestions.Select(s => s.Id).Intersect(store.CurrentUser.FollowingIds));
    }

    [Fact]
    public void Suggestions_respect_the_limit()
    {
        var (_, users, _, _) = Build();

        Assert.True(Body(users.GetSuggestions(limit: 1)).Count <= 1);
    }

    [Fact]
    public void Followers_and_following_lists_agree_with_each_other()
    {
        var (_, users, _, store) = Build();

        var me = store.CurrentUser;
        var following = Body(users.GetFollowing(me.Username));

        foreach (var person in following)
        {
            var theirFollowers = Body(users.GetFollowers(person.Username));
            Assert.Contains(me.Id, theirFollowers.Select(f => f.Id));
        }
    }

    [Fact]
    public void Marking_a_story_seen_sticks()
    {
        var (_, _, stories, _) = Build();

        var unseen = Body(stories.GetStories()).First(s => !s.Seen);

        Assert.True(Body(stories.MarkSeen(unseen.Id)).Seen);
        Assert.True(Body(stories.GetStories()).Single(s => s.Id == unseen.Id).Seen);
    }

    [Fact]
    public void Stories_list_puts_unseen_first()
    {
        var (_, _, stories, _) = Build();

        var seenFlags = Body(stories.GetStories()).Select(s => s.Seen).ToList();

        Assert.Equal(seenFlags.OrderBy(s => s), seenFlags);
    }

    [Fact]
    public void Explore_returns_everything_including_posts_you_do_not_follow()
    {
        var (posts, _, _, store) = Build();

        var explore = Body(posts.GetExplore());

        Assert.Equal(store.Posts.Count(), explore.Count);
    }

    [Fact]
    public void Feed_only_carries_a_preview_of_comments_while_post_detail_carries_all()
    {
        var (posts, _, _, store) = Build();

        // Pick a post with more comments than the inline preview shows.
        var chatty = store.Posts
            .Where(p => p.Comments.Count > 2)
            .OrderByDescending(p => p.Comments.Count)
            .FirstOrDefault();

        if (chatty is null)
        {
            // Seed data happened not to produce one; the assertion below is then vacuous.
            return;
        }

        var detail = Body(posts.GetPost(chatty.Id));
        Assert.Equal(chatty.Comments.Count, detail.Comments.Count);

        var explore = Body(posts.GetExplore()).Single(p => p.Id == chatty.Id);
        Assert.True(explore.Comments.Count < explore.CommentCount);
    }
}
