﻿using OnlineEducation.DTO;
using Microsoft.AspNetCore.Mvc;
using OnlineEducation.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace OnlineEducation.Controllers
{
    [Authorize]
    [ApiController]
    [Produces("application/json")]
    [Route("api")]
    public class UsersController : ControllerBase
    {
        private IStudentService _userService;

        public UsersController(IStudentService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody]AuthenticateModel model)
        {
            var user = await _userService.Authenticate(model.Username, model.Password);

            if (user == null)
                return Unauthorized(new { message = "Username or password is incorrect" });

            return Ok(user);
        }
    }
}
