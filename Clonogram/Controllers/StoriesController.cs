using System;
using System.Threading.Tasks;
using Clonogram.Services;
using Clonogram.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clonogram.Controllers
{
    [Authorize]
    public class StoriesController : ControllerBase
    {
        private readonly IStoriesService _storiesService;

        public StoriesController(IStoriesService storiesService)
        {
            _storiesService = storiesService;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(StoryView story)
        {
            var file = HttpContext.Request.Form.Files[0];
            story.UserId = HttpContext.User.Identity.Name;

            await _storiesService.Upload(file, story);

            return Ok();
        }

        public async Task<IActionResult> GetById(string id)
        {
            var guid = Guid.Parse(id);
            var storyView = await _storiesService.GetById(guid);
            return Ok(storyView);
        }

        public async Task<IActionResult> GetStoriesByUser(string userId)
        {
            var guid = Guid.Parse(userId);
            var stories = await _storiesService.GetAllStories(guid);
            return Ok(stories);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var userId = Guid.Parse(HttpContext.User.Identity.Name);
                var storyId = Guid.Parse(id);
                await _storiesService.Delete(userId, storyId);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
