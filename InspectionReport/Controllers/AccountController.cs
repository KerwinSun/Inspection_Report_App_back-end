using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using InspectionReport.Models;
using InspectionReport.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace InspectionReport.Controllers
{
    //[Authorize]
    [Route("api/auth/[action]")]
    public class AccountController : Controller
    {
        private UserManager<ApplicationUser> _userManager = null;
        private SignInManager<ApplicationUser> _signInManager = null;
        private ReportContext _context = null;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ReportContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

/* 
        // GET: Dummy method to create a new user.
        [HttpGet]
        [AllowAnonymous]
        [ActionName("dummypost")]
        public async Task<IEnumerable<string>> CreateUserAsync()
        {
            ApplicationUser appUser = new ApplicationUser()
            {
                UserName = "rob",
                Email = "rob@rob.com"
            };

            var result = await _userManager.CreateAsync(appUser, "Test123!");
            if (result.Succeeded)
            {
                User user = new User()
                {
                    FirstName = "Rob",
                    LastName = "Kirkpatrick",
                    AppLoginUser = appUser
                };

                user.AppLoginUser = appUser;

                _context.User.Add(user);

                return new string[] { appUser.Id, appUser.UserName };
            }
            return new string[] { "Not Created" };
        }
        */

        // POST createAcc
        [HttpPost]
        [AllowAnonymous]
        [ActionName("createAccount")]
        public async Task<IActionResult> CreateUserAsync([FromBody] User user) {
            
            if (user == null) {
                return BadRequest();
            }
            
            ApplicationUser appUser = new ApplicationUser() {
                UserName = user.Email,
                Email = user.Email
            };

            user.AppLoginUser = appUser;

            var result = await _userManager.CreateAsync(user.AppLoginUser, user.Password);
            if (result.Succeeded) {
                user.Password = "";
                _context.User.Add(user);
                _context.SaveChanges();
                // return new string[] { user.AppLoginUser.Id, user.AppLoginUser.UserName};
                return Content("User was created", "text/html");
            }
            return Content("User created failed", "text/html");
        }

        // POST api/login
        [HttpPost]
        [AllowAnonymous]
        [ActionName("login")]
        public async Task<IActionResult> SignIn([FromBody]LoginModel model)
        {
            // Get user and check credentials
            ApplicationUser user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized(); //Email not found.
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
            if (result.Succeeded)
            {
                var currentUser = _context.User.Where(u => u.AppLoginUser == user).SingleOrDefault();
                return Ok(currentUser);
            }
            else
            {
                return Unauthorized();
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
