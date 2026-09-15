using Instagram.Api.Data;

namespace Instagram.Api.Tests;

public class DummyDataStoreTests
{
    [Fact]
    public void Seeds_users_posts_and_stories()
    {
        var store = new DummyDataStore();

        Assert.NotEmpty(store.Users);
        Assert.NotEmpty(store.Posts);
        Assert.NotEmpty(store.Stories);
    }

    [Fact]
    public void Current_user_resolves_to_a_seeded_user()
    {
        var store = new DummyDataStore();

        Assert.NotNull(store.CurrentUser);
        Assert.Equal(store.CurrentUserId, store.CurrentUser.Id);
    }

    [Fact]
    public void Current_user_has_posts_so_their_profile_is_not_empty()
    {
        var store = new DummyDataStore();

        Assert.True(store.CountPosts(store.CurrentUserId) > 0);
    }

    [Fact]
    public void Every_post_and_comment_has_a_known_author()
    {
        var store = new DummyDataStore();

        foreach (var post in store.Posts)
        {
            Assert.NotNull(store.FindUser(post.AuthorId));

            foreach (var comment in post.Comments)
            {
                Assert.NotNull(store.FindUser(comment.AuthorId));
                Assert.Equal(post.Id, comment.PostId);
            }
        }
    }

    [Fact]
    public void No_post_or_comment_is_dated_in_the_future()
    {
        var store = new DummyDataStore();
        // A second of slack absorbs the time spent seeding.
        var ceiling = DateTimeOffset.UtcNow.AddSeconds(1);

        foreach (var post in store.Posts)
        {
            Assert.True(post.CreatedAt <= ceiling, $"{post.Id} is dated in the future.");

            foreach (var comment in post.Comments)
            {
                Assert.True(comment.CreatedAt <= ceiling, $"{comment.Id} is dated in the future.");
                // A comment cannot predate the post it belongs to.
                Assert.True(comment.CreatedAt >= post.CreatedAt, $"{comment.Id} predates its post.");
            }
        }
    }

    [Fact]
    public void Nobody_likes_their_own_post_and_nobody_follows_themselves()
    {
        var store = new DummyDataStore();

        foreach (var post in store.Posts)
        {
            Assert.DoesNotContain(post.AuthorId, post.LikedByIds);
        }

        foreach (var user in store.Users)
        {
            Assert.DoesNotContain(user.Id, user.FollowingIds);
        }
    }

    [Fact]
    public void Seeded_stories_have_not_expired()
    {
        var store = new DummyDataStore();
        var now = DateTimeOffset.UtcNow;

        Assert.All(store.Stories, story => Assert.True(story.ExpiresAt > now));
    }

    [Fact]
    public void Usernames_are_unique_and_lookup_is_case_insensitive()
    {
        var store = new DummyDataStore();

        var usernames = store.Users.Select(u => u.Username).ToList();
        Assert.Equal(usernames.Count, usernames.Distinct(StringComparer.OrdinalIgnoreCase).Count());

        var first = usernames.First();
        Assert.NotNull(store.FindUserByUsername(first.ToUpperInvariant()));
    }

    [Fact]
    public void Follower_count_matches_who_actually_follows()
    {
        var store = new DummyDataStore();
        var target = store.Users.First(u => u.Id != store.CurrentUserId);

        var expected = store.Users.Count(u => u.FollowingIds.Contains(target.Id));

        Assert.Equal(expected, store.CountFollowers(target.Id));
    }

    [Fact]
    public void Seeding_is_deterministic_across_instances()
    {
        // A fixed random seed keeps the demo feed stable between runs.
        var a = new DummyDataStore();
        var b = new DummyDataStore();

        Assert.Equal(
            a.Posts.OrderBy(p => p.Id).Select(p => (p.Id, p.AuthorId, p.LikedByIds.Count)),
            b.Posts.OrderBy(p => p.Id).Select(p => (p.Id, p.AuthorId, p.LikedByIds.Count)));
    }
}
