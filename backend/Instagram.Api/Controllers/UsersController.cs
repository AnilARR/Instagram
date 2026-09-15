using Instagram.Api.Data;
using Instagram.Api.Dtos;
using Instagram.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Instagram.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(DummyDataStore store, DtoMapper mapper) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<UserSummaryDto>> GetUsers()
    {
        lock (store.Gate)
        {
            var users = store.Users
                .OrderBy(u => u.Username)
                .Select(mapper.ToSummary)
                .ToList();

            return Ok(users);
        }
    }

    /// <summary>The account the API answers as — there is no login, so this is fixed.</summary>
    [HttpGet("me")]
    public ActionResult<UserProfileDto> GetCurrentUser()
    {
        lock (store.Gate)
        {
            return Ok(mapper.ToProfile(store.CurrentUser));
        }
    }

    /// <summary>People the current user does not follow yet.</summary>
    [HttpGet("suggestions")]
    public ActionResult<IReadOnlyList<UserSummaryDto>> GetSuggestions([FromQuery] int limit = 5)
    {
        limit = Math.Clamp(limit, 1, 20);

        lock (store.Gate)
        {
            var me = store.CurrentUser;

            var suggestions = store.Users
                .Where(u => u.Id != me.Id && !me.FollowingIds.Contains(u.Id))
                // Most-followed first, so suggestions feel plausible.
                .OrderByDescending(u => store.CountFollowers(u.Id))
                .Take(limit)
                .Select(mapper.ToSummary)
                .ToList();

            return Ok(suggestions);
        }
    }

    [HttpGet("{username}")]
    public ActionResult<UserProfileDto> GetProfile(string username)
    {
        lock (store.Gate)
        {
            var user = store.FindUserByUsername(username);
            if (user is null)
            {
                return NotFound();
            }

            return Ok(mapper.ToProfile(user));
        }
    }

    /// <summary>The user's own posts, newest first — the profile grid.</summary>
    [HttpGet("{username}/posts")]
    public ActionResult<IReadOnlyList<PostDto>> GetUserPosts(string username)
    {
        lock (store.Gate)
        {
            var user = store.FindUserByUsername(username);
            if (user is null)
            {
                return NotFound();
            }

            var posts = store.Posts
                .Where(p => p.AuthorId == user.Id)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => mapper.ToDto(p))
                .ToList();

            return Ok(posts);
        }
    }

    /// <summary>Toggles whether the current user follows this account.</summary>
    [HttpPost("{username}/follow")]
    public ActionResult<FollowResultDto> ToggleFollow(string username)
    {
        lock (store.Gate)
        {
            var user = store.FindUserByUsername(username);
            if (user is null)
            {
                return NotFound();
            }

            var me = store.CurrentUser;
            if (user.Id == me.Id)
            {
                return BadRequest(new { message = "You cannot follow yourself." });
            }

            if (!me.FollowingIds.Remove(user.Id))
            {
                me.FollowingIds.Add(user.Id);
            }

            return Ok(new FollowResultDto(
                user.Id,
                store.CountFollowers(user.Id),
                me.FollowingIds.Contains(user.Id)));
        }
    }

    [HttpGet("{username}/followers")]
    public ActionResult<IReadOnlyList<UserSummaryDto>> GetFollowers(string username)
    {
        lock (store.Gate)
        {
            var user = store.FindUserByUsername(username);
            if (user is null)
            {
                return NotFound();
            }

            var followers = store.Users
                .Where(u => u.FollowingIds.Contains(user.Id))
                .OrderBy(u => u.Username)
                .Select(mapper.ToSummary)
                .ToList();

            return Ok(followers);
        }
    }

    [HttpGet("{username}/following")]
    public ActionResult<IReadOnlyList<UserSummaryDto>> GetFollowing(string username)
    {
        lock (store.Gate)
        {
            var user = store.FindUserByUsername(username);
            if (user is null)
            {
                return NotFound();
            }

            var following = user.FollowingIds
                .Select(store.FindUser)
                .Where(u => u is not null)
                .OrderBy(u => u!.Username)
                .Select(u => mapper.ToSummary(u!))
                .ToList();

            return Ok(following);
        }
    }
}
