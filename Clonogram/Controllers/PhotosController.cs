using System;
using System.Threading.Tasks;
using Clonogram.Services;
using Clonogram.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clonogram.Controllers
{
    [Authorize]
    public class PhotosController : ControllerBase
    {
        private readonly IPhotoService _photoService;

        public PhotosController(IPhotoService photoService)
        {
            _photoService = photoService;
        }

        [HttpPost]
        public IActionResult Upload(PhotoView photo)
        {
            var file = HttpContext.Request.Form.Files[0];
            photo.UserId = HttpContext.User.Identity.Name;

            _photoService.Upload(file, photo);

            return Ok();
        }

        public async Task<IActionResult> GetById(string id)
        {
            var guid = Guid.Parse(id);
            var photoView = await _photoService.GetById(guid);
            return Ok(photoView);
        }

        [HttpPut]
        public async Task<IActionResult> Update(PhotoView photoView)
        {
            try
            {
                photoView.Id = HttpContext.User.Identity.Name;
                await _photoService.Update(photoView);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var userId = Guid.Parse(HttpContext.User.Identity.Name);
                var photoId = Guid.Parse(id);
                await _photoService.Delete(userId, photoId);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
