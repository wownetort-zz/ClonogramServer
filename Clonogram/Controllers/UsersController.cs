using System;
using System.Threading.Tasks;
using Clonogram.Services;
using Clonogram.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;

namespace Clonogram.Controllers
{
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUsersService _usersService;
        private readonly IJWTService _jwtService;

        public UsersController(IUsersService usersService, IJWTService jwtService)
        {
            _usersService = usersService;
            _jwtService = jwtService;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Authenticate(string username, string password)
        {
            var user = await _usersService.Authenticate(username, password);
            if (user == null) return BadRequest(new { message = "Username or password is incorrect" });

            var tokenString = _jwtService.GetToken(user.Id);

            return Ok(new
            {
                user,
                tokenString
            });
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(UserView userView)
        {
            try
            {
                var avatar = HttpContext.Request.Form.Files.Any() ? HttpContext.Request.Form.Files[0] : null;
                await _usersService.Create(userView, avatar);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        public async Task<IActionResult> GetAllUsersByName(string name)
        {
            try
            {
                var users = await _usersService.GetAllUsersByName(name);
                return Ok(users);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        public async Task<IActionResult> GetAllSubscribers()
        {
            var userId = Guid.Parse(HttpContext.User.Identity.Name);
            var users = await _usersService.GetAllSubscribers(userId);
            return Ok(users);
        }

        public async Task<IActionResult> GetAllSubscriptions()
        {
            var userId = Guid.Parse(HttpContext.User.Identity.Name);
            var users = await _usersService.GetAllSubscriptions(userId);
            return Ok(users);
        }

        public async Task<IActionResult> GetById(string id)
        {
            var guid = Guid.Parse(id);
            var userView = await _usersService.GetById(guid);
            return Ok(userView);
        }

        [HttpPut]
        public async Task<IActionResult> Update(UserView userView)
        {
            try
            {
                var avatar = HttpContext.Request.Form.Files.Any() ? HttpContext.Request.Form.Files[0] : null;
                userView.Id = HttpContext.User.Identity.Name;
                await _usersService.Update(userView, avatar);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            var guid = Guid.Parse(HttpContext.User.Identity.Name);
            await _usersService.Delete(guid);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Subscribe(string userId)
        {
            try
            {
                if (userId == HttpContext.User.Identity.Name) return BadRequest(new { message = "Can't subscribe on yourself" });
                var userGuid = Guid.Parse(HttpContext.User.Identity.Name);
                var secondaryGuid = Guid.Parse(userId);

                await _usersService.Subscribe(userGuid, secondaryGuid);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}