using Instagram.Api.Models;

namespace Instagram.Api.Data;

/// <summary>
/// The whole "database": a thread-safe in-memory graph of users, posts,
/// comments and stories, seeded deterministically so every run of the app
/// shows the same feed. Registered as a singleton, so writes (likes, comments,
/// follows, new posts) survive for the lifetime of the process and are lost on
/// restart — which is exactly what a dummy project wants.
/// </summary>
public class DummyDataStore
{
    private readonly Dictionary<string, User> _users = new();
    private readonly Dictionary<string, Post> _posts = new();
    private readonly Dictionary<string, Story> _stories = new();

    /// <summary>Guards every read and write; the seeded set is small so a single lock is plenty.</summary>
    public readonly object Gate = new();

    /// <summary>
    /// The user the API answers as. With no authentication, "who am I" has to
    /// come from somewhere, so the first seeded user plays that part.
    /// </summary>
    public string CurrentUserId { get; } = "u1";

    public DummyDataStore() => Seed();

    public IEnumerable<User> Users => _users.Values;
    public IEnumerable<Post> Posts => _posts.Values;
    public IEnumerable<Story> Stories => _stories.Values;

    public User? FindUser(string id) => _users.GetValueOrDefault(id);

    public User? FindUserByUsername(string username) =>
        _users.Values.FirstOrDefault(u =>
            string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

    public Post? FindPost(string id) => _posts.GetValueOrDefault(id);

    public Story? FindStory(string id) => _stories.GetValueOrDefault(id);

    public User CurrentUser => _users[CurrentUserId];

    public void AddPost(Post post) => _posts[post.Id] = post;

    /// <summary>Everyone who follows <paramref name="userId"/>.</summary>
    public int CountFollowers(string userId) =>
        _users.Values.Count(u => u.FollowingIds.Contains(userId));

    public int CountPosts(string userId) =>
        _posts.Values.Count(p => p.AuthorId == userId);

    // ---------------------------------------------------------------- seeding

    private void Seed()
    {
        // Fixed seed keeps like counts and timestamps stable between runs.
        var random = new Random(20240517);
        var now = DateTimeOffset.UtcNow;

        var people = new (string Id, string Username, string FullName, string Bio, string Website, bool Verified)[]
        {
            ("u1", "you", "Your Name", "Just a dummy account for the demo 👋", "example.com", false),
            ("u2", "maya.codes", "Maya Fernandes", "Frontend dev · Angular & TypeScript", "maya.dev", true),
            ("u3", "trailsandtea", "Nikhil Rao", "Weekend hiker. Weekday debugger.", "", false),
            ("u4", "the.slow.bake", "Priya Menon", "Sourdough, mostly. Occasionally cake.", "slowbake.example", false),
            ("u5", "analogue.aki", "Akira Tanaka", "35mm · Tokyo → Lisbon", "akiframes.example", true),
            ("u6", "greenhouse.notes", "Sam Oyelaran", "Too many plants. Zero regrets.", "", false),
            ("u7", "dmitri.builds", "Dmitri Volkov", ".NET backend. Coffee-powered.", "", false),
            ("u8", "lina.paints", "Lina Haddad", "Gouache & tiny studies 🎨", "linahaddad.example", false),
        };

        foreach (var p in people)
        {
            _users[p.Id] = new User
            {
                Id = p.Id,
                Username = p.Username,
                FullName = p.FullName,
                AvatarUrl = $"https://i.pravatar.cc/200?u={p.Username}",
                Bio = p.Bio,
                Website = p.Website,
                IsVerified = p.Verified,
            };
        }

        // The current user follows most people; the rest follow each other in a
        // loop so follower counts are never all zero.
        foreach (var id in new[] { "u2", "u3", "u4", "u5", "u6" })
        {
            _users["u1"].FollowingIds.Add(id);
        }

        var ids = people.Select(p => p.Id).ToArray();
        for (var i = 0; i < ids.Length; i++)
        {
            // Follow the next three users in the ring, skipping yourself.
            for (var step = 1; step <= 3; step++)
            {
                var target = ids[(i + step) % ids.Length];
                if (target != ids[i])
                {
                    _users[ids[i]].FollowingIds.Add(target);
                }
            }
        }

        var seededPosts = new (string AuthorId, string Caption, string Location)[]
        {
            ("u5", "Rolled the last frame of the trip on the tram home. Worth it.", "Lisbon, Portugal"),
            ("u4", "Third attempt at a 75% hydration loaf. The crumb finally behaved.", ""),
            ("u3", "Six hours up, twenty minutes of this. Fair trade.", "Western Ghats"),
            ("u2", "Signals everywhere and I am never going back. ⚡", "Porto, Portugal"),
            ("u6", "The monstera has opinions about where it lives now.", ""),
            ("u8", "Small study, 10×15cm. Painted before the light moved.", "Beirut, Lebanon"),
            ("u7", "Minimal APIs and a very large mug.", ""),
            ("u5", "Shot this on expired film and it came back better than expected.", "Tokyo, Japan"),
            ("u3", "Found a lake the map did not mention.", ""),
            ("u4", "Cardamom buns for a rainy Sunday.", ""),
            ("u2", "Refactored the whole feed into signals. Diff was brutal, result is clean.", ""),
            ("u6", "Propagation station finally earning its keep.", ""),
            // A couple of posts by the current user, so their own profile grid
            // is not empty on first load.
            ("u1", "First post on the dummy app. Hello 👋", ""),
            ("u1", "Testing the upload flow with a very serious photograph.", "Somewhere"),
        };

        var commentPool = new[]
        {
            "This is beautiful 😍", "okay but HOW", "the colours here!!",
            "saving this one", "instant favourite", "teach me your ways",
            "no notes. perfect.", "criminally underrated shot",
            "the composition though", "I need this on a wall",
            "absolutely stealing this idea", "this made my morning",
        };

        for (var i = 0; i < seededPosts.Length; i++)
        {
            var (authorId, caption, location) = seededPosts[i];
            var postId = $"p{i + 1}";
            // Newest post first: index 0 is the most recent.
            var createdAt = now.AddHours(-(i * 7 + random.Next(0, 5)));

            var post = new Post
            {
                Id = postId,
                AuthorId = authorId,
                ImageUrl = $"https://picsum.photos/seed/{postId}-ig/900/900",
                Caption = caption,
                Location = location,
                CreatedAt = createdAt,
            };

            // Spread likes across everyone except the author.
            foreach (var candidate in _users.Values.Where(u => u.Id != authorId))
            {
                if (random.Next(0, 100) < 65)
                {
                    post.LikedByIds.Add(candidate.Id);
                }
            }

            var commentCount = random.Next(1, 4);
            for (var c = 0; c < commentCount; c++)
            {
                var commenter = _users.Values
                    .Where(u => u.Id != authorId)
                    .ElementAt(random.Next(0, _users.Count - 1));

                // Land the comment somewhere between the post and now, so a
                // freshly seeded post never carries a comment from the future.
                var windowMinutes = (int)Math.Min((now - createdAt).TotalMinutes, 600);
                var offsetMinutes = windowMinutes > 1 ? random.Next(1, windowMinutes) : 0;

                post.Comments.Add(new Comment
                {
                    Id = $"{postId}-c{c + 1}",
                    PostId = postId,
                    AuthorId = commenter.Id,
                    Text = commentPool[random.Next(0, commentPool.Length)],
                    CreatedAt = createdAt.AddMinutes(offsetMinutes),
                });
            }

            _posts[postId] = post;
        }

        // A handful of live stories, all within the 24h window.
        var storyAuthors = new[] { "u2", "u3", "u4", "u5", "u6", "u8" };
        for (var i = 0; i < storyAuthors.Length; i++)
        {
            var storyId = $"s{i + 1}";
            _stories[storyId] = new Story
            {
                Id = storyId,
                AuthorId = storyAuthors[i],
                ImageUrl = $"https://picsum.photos/seed/{storyId}-story/720/1280",
                CreatedAt = now.AddHours(-random.Next(1, 20)),
                Seen = i >= 4,
            };
        }
    }
}
