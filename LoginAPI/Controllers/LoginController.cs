using LoginAPI.Context;
using LoginAPI.Models;
using LoginAPI.Services;
using LoginAPI.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;

namespace LoginAPI.Controllers
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IDbContextFactory<ApiContext> dbContextFactory;
        private readonly TokenService tokenService;

        public LoginController(IDbContextFactory<ApiContext> dbContextFactory,
            TokenService tokenService)
        {
            this.dbContextFactory = dbContextFactory;
            this.tokenService = tokenService;
        }

        [HttpPost]
        public IActionResult Login(User user)
        {
            try
            {
                using var db = dbContextFactory.CreateDbContext();
                var localUser = db.Users.Where(x => x.Email == user.Email).FirstOrDefault();

                if (localUser == null ||
                    localUser.Password != user.Password.GetMD5())
                    return BadRequest("Email or password incorrect");

                localUser.Password = null;

                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, localUser.Name),
                    new Claim(ClaimTypes.UserData, localUser.ToJson()),
                    new Claim(ClaimTypes.Role, "User"),
                };

                var token = tokenService.GenerateToken(claims);

                return Ok(new { auth = token, user = localUser });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.InnerException?.Message ?? e.Message);
            }
        }
    }
}
