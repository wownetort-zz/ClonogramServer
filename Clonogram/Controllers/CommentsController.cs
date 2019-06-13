using System;
using System.Threading.Tasks;
using Clonogram.Services;
using Clonogram.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Clonogram.Controllers
{
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CommentView commentView)
        {
            commentView.UserId = HttpContext.User.Identity.Name;

            await _commentService.Create(commentView);
            return Ok();
        }

        public async Task<IActionResult> GetById(string id)
        {
            var guid = Guid.Parse(id);
            var commentView = await _commentService.GetById(guid);
            return Ok(commentView);
        }

        [HttpPut]
        public async Task<IActionResult> Update(CommentView commentView)
        {
            try
            {
                commentView.UserId = HttpContext.User.Identity.Name;
                await _commentService.Update(commentView);
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
                await _commentService.DeleteMy(userId, commentId);
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
                await _commentService.DeleteOnMyPhoto(userId, commentId);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
