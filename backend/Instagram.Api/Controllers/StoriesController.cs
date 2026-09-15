using Instagram.Api.Data;
using Instagram.Api.Dtos;
using Instagram.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Instagram.Api.Controllers;

[ApiController]
[Route("api/stories")]
public class StoriesController(DummyDataStore store, DtoMapper mapper) : ControllerBase
{
    /// <summary>
    /// Live stories (unexpired) from the people the current user follows,
    /// unseen ones first so the tray reads the way Instagram's does.
    /// </summary>
    [HttpGet]
    public ActionResult<IReadOnlyList<StoryDto>> GetStories()
    {
        lock (store.Gate)
        {
            var me = store.CurrentUser;
            var now = DateTimeOffset.UtcNow;

            var stories = store.Stories
                .Where(s => s.ExpiresAt > now)
                .Where(s => me.FollowingIds.Contains(s.AuthorId) || s.AuthorId == me.Id)
                .OrderBy(s => s.Seen)
                .ThenByDescending(s => s.CreatedAt)
                .Select(mapper.ToDto)
                .ToList();

            return Ok(stories);
        }
    }

    /// <summary>Marks a story as watched.</summary>
    [HttpPost("{id}/seen")]
    public ActionResult<StoryDto> MarkSeen(string id)
    {
        lock (store.Gate)
        {
            var story = store.FindStory(id);
            if (story is null)
            {
                return NotFound();
            }

            story.Seen = true;
            return Ok(mapper.ToDto(story));
        }
    }
}
