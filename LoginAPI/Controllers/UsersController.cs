using LoginAPI.Context;
using LoginAPI.Models;
using LoginAPI.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LoginAPI.Controllers
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IDbContextFactory<ApiContext> dbContextFactory;

        public UsersController(IDbContextFactory<ApiContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        [HttpGet("{id}")]
        public ActionResult<List<User>> Get(int id)
        {
            try
            {
                if (id == 0)
                    return BadRequest();

                using var db = dbContextFactory.CreateDbContext();
                var user = db.Users.Where(x => x.Id == id).FirstOrDefault();

                if (user == null)
                    return NotFound();

                user.Password = null;
                return Ok(user);

            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.InnerException?.Message ?? e.Message);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Post(User user)
        {
            try
            {
                using var db = dbContextFactory.CreateDbContext();
                user.Password = user.Password.GetMD5();
                user.CreatedAt = DateTime.Now;
                await db.Users.AddAsync(user);
                await db.SaveChangesAsync();

                return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.InnerException?.Message ?? e.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, User user)
        {
            try
            {
                if (id != user.Id)
                    return BadRequest();

                using var db = dbContextFactory.CreateDbContext();
                var localUser = db.Users.Where(x => x.Id == id).FirstOrDefault();

                if (localUser == null)
                    return NotFound();

                if (user.Password.IsNotEmpty())
                    user.Password = user.Password.GetMD5();

                db.Entry(user).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.InnerException?.Message ?? e.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id == 0)
                    return BadRequest();

                using var db = dbContextFactory.CreateDbContext();
                var localUser = db.Users.Where(x => x.Id == id).FirstOrDefault();

                if (localUser == null)
                    return NotFound();

                db.Users.Remove(localUser);
                await db.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.InnerException?.Message ?? e.Message);
            }
        }
    }
}
