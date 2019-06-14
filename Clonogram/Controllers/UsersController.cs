using System;
using System.Threading.Tasks;
using Clonogram.Services;
using Clonogram.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clonogram.Controllers
{
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJWTService _jwtService;

        public UsersController(IUserService userService, IJWTService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Authenticate(string username, string password)
        {
            var user = await _userService.Authenticate(username, password);
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
                await _userService.Create(userView);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        public async Task<IActionResult> GetAllUsernames()
        {
            var users = await _userService.GetAllUsernames();
            return Ok(users);
        }

        public async Task<IActionResult> GetById(string id)
        {
            var guid = Guid.Parse(id);
            var userView = await _userService.GetById(guid);
            return Ok(userView);
        }

        [HttpPut]
        public async Task<IActionResult> Update(UserView userView)
        {
            try
            {
                userView.Id = HttpContext.User.Identity.Name;
                await _userService.Update(userView);
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
            await _userService.Delete(guid);
            return Ok();
        }
    }
}