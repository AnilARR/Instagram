using Instagram.Api.Data;
using Instagram.Api.Dtos;
using Instagram.Api.Models;
using Instagram.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Instagram.Api.Controllers;

[ApiController]
[Route("api/posts")]
public class PostsController(DummyDataStore store, DtoMapper mapper) : ControllerBase
{
    /// <summary>
    /// The home feed: posts by the people the current user follows, plus their
    /// own, newest first.
    /// </summary>
    [HttpGet]
    public ActionResult<PagedResult<PostDto>> GetFeed(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 6)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        lock (store.Gate)
        {
            var me = store.CurrentUser;
            var visibleAuthors = new HashSet<string>(me.FollowingIds) { me.Id };

            var feed = store.Posts
                .Where(p => visibleAuthors.Contains(p.AuthorId))
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            var items = feed
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => mapper.ToDto(p))
                .ToList();

            return Ok(new PagedResult<PostDto>(items, page, pageSize, feed.Count));
        }
    }

    /// <summary>Every post in the store, regardless of who is followed — the "explore" grid.</summary>
    [HttpGet("explore")]
    public ActionResult<IReadOnlyList<PostDto>> GetExplore()
    {
        lock (store.Gate)
        {
            var items = store.Posts
                .OrderByDescending(p => p.LikedByIds.Count)
                .Select(p => mapper.ToDto(p))
                .ToList();

            return Ok(items);
        }
    }

    [HttpGet("{id}")]
    public ActionResult<PostDto> GetPost(string id)
    {
        lock (store.Gate)
        {
            var post = store.FindPost(id);
            if (post is null)
            {
                return NotFound();
            }

            return Ok(mapper.ToDto(post, allComments: true));
        }
    }

    /// <summary>Toggles the current user's like, and reports the resulting state.</summary>
    [HttpPost("{id}/like")]
    public ActionResult<LikeResultDto> ToggleLike(string id)
    {
        lock (store.Gate)
        {
            var post = store.FindPost(id);
            if (post is null)
            {
                return NotFound();
            }

            var me = store.CurrentUserId;
            if (!post.LikedByIds.Remove(me))
            {
                post.LikedByIds.Add(me);
            }

            return Ok(new LikeResultDto(post.Id, post.LikedByIds.Count, post.LikedByIds.Contains(me)));
        }
    }

    [HttpGet("{id}/comments")]
    public ActionResult<IReadOnlyList<CommentDto>> GetComments(string id)
    {
        lock (store.Gate)
        {
            var post = store.FindPost(id);
            if (post is null)
            {
                return NotFound();
            }

            var comments = post.Comments
                .OrderBy(c => c.CreatedAt)
                .Select(mapper.ToDto)
                .ToList();

            return Ok(comments);
        }
    }

    [HttpPost("{id}/comments")]
    public ActionResult<CommentDto> AddComment(string id, [FromBody] CreateCommentRequest request)
    {
        var text = request.Text?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            return BadRequest(new { message = "Comment text is required." });
        }

        lock (store.Gate)
        {
            var post = store.FindPost(id);
            if (post is null)
            {
                return NotFound();
            }

            var comment = new Comment
            {
                Id = $"{post.Id}-c{Guid.NewGuid():N}",
                PostId = post.Id,
                AuthorId = store.CurrentUserId,
                Text = text,
                CreatedAt = DateTimeOffset.UtcNow,
            };

            post.Comments.Add(comment);

            return CreatedAtAction(nameof(GetComments), new { id = post.Id }, mapper.ToDto(comment));
        }
    }

    [HttpDelete("{postId}/comments/{commentId}")]
    public IActionResult DeleteComment(string postId, string commentId)
    {
        lock (store.Gate)
        {
            var post = store.FindPost(postId);
            var comment = post?.Comments.FirstOrDefault(c => c.Id == commentId);
            if (post is null || comment is null)
            {
                return NotFound();
            }

            // Only your own comments are yours to remove.
            if (comment.AuthorId != store.CurrentUserId)
            {
                return Forbid();
            }

            post.Comments.Remove(comment);
            return NoContent();
        }
    }

    /// <summary>Publishes a post as the current user.</summary>
    [HttpPost]
    public ActionResult<PostDto> CreatePost([FromBody] CreatePostRequest request)
    {
        var imageUrl = request.ImageUrl?.Trim();
        if (string.IsNullOrEmpty(imageUrl))
        {
            return BadRequest(new { message = "An image URL is required." });
        }

        lock (store.Gate)
        {
            var post = new Post
            {
                Id = $"p{Guid.NewGuid():N}",
                AuthorId = store.CurrentUserId,
                ImageUrl = imageUrl,
                Caption = request.Caption?.Trim() ?? string.Empty,
                Location = request.Location?.Trim() ?? string.Empty,
                CreatedAt = DateTimeOffset.UtcNow,
            };

            store.AddPost(post);

            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, mapper.ToDto(post, allComments: true));
        }
    }
}
