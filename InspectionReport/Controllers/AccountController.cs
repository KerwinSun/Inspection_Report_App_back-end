using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InspectionReport.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace InspectionReport.Controllers
{
    [Authorize]
    [Route("api/auth/[action]")]
    public class AccountController : Controller
    {
        private UserManager<ApplicationUser> _userManager = null;
        private SignInManager<ApplicationUser> _signInManager = null;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: Dummy method to create a new user.
        [HttpGet]
        [AllowAnonymous]
        [ActionName("dummypost")]
        public async Task<IEnumerable<string>> CreateUserAsync()
        {
            ApplicationUser user = new ApplicationUser()
            {
                UserName = "bob",
                Email = "bob@bob.com"
            };

            var result = await _userManager.CreateAsync(user, "Test123!");
            if (result.Succeeded)
            {
                return new string[] { user.Id, user.UserName };
            }
            return new string[] { "Not Created" };
        }

        // POST api/login
        [HttpPost]
        [AllowAnonymous]
        [ActionName("login")]
        public async Task<IActionResult> SignIn([FromBody]LoginViewModel model)
        {
            // TODO: Sign out to clean up in-case user is attempting to sign-in while already signed-in.
            // Get user and check credentials
            var user = await _userManager.FindByEmailAsync(model.Email);
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, true, false);
            if (result.Succeeded)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        // POST api/<controller>
        [HttpPost]
        [ActionName("logout")]
        public async Task<IActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }
    }
}
