using System;
using System.Threading.Tasks;
using Clonogram.Services;
using Clonogram.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Clonogram.Controllers
{
    public class CommentsController : ControllerBase
    {
        private readonly ICommentsService _commentsService;

        public CommentsController(ICommentsService commentsService)
        {
            _commentsService = commentsService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CommentView commentView)
        {
            try
            {
                commentView.UserId = HttpContext.User.Identity.Name;
                await _commentsService.Create(commentView);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        public async Task<IActionResult> GetById(string id)
        {
            var guid = Guid.Parse(id);
            var commentView = await _commentsService.GetById(guid);
            return Ok(commentView);
        }

        public async Task<IActionResult> GetAllPhotoComments(string photoId)
        {
            var guid = Guid.Parse(photoId);
            var comments = await _commentsService.GetAllPhotosComments(guid);
            return Ok(comments);
        }

        [HttpPut]
        public async Task<IActionResult> Update(CommentView commentView)
        {
            try
            {
                commentView.UserId = HttpContext.User.Identity.Name;
                await _commentsService.Update(commentView);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMy(string id)
        {
            try
            {
                var userId = Guid.Parse(HttpContext.User.Identity.Name);
                var commentId = Guid.Parse(id);
                await _commentsService.DeleteMy(userId, commentId);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteOnMyPhoto(string id)
        {
            try
            {
                var userId = Guid.Parse(HttpContext.User.Identity.Name);
                var commentId = Guid.Parse(id);
                await _commentsService.DeleteOnMyPhoto(userId, commentId);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
