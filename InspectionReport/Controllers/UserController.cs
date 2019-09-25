﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Mvc;
using InspectionReport.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace InspectionReport.Controllers
{
    //[Authorize]
    [Route("api/User")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private UserManager<ApplicationUser> _userManager = null;
        private readonly ReportContext _context;

        public UserController(ReportContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _context.User.ToList();
            foreach (User user in users){
                user.Password = "";
            }

            return Ok(users);
        }

        [HttpGet("{id}", Name = "GetUser")]
        public IActionResult GetById(long id)
        {
            User user = _context.User
                            .Include(u => u.Inspected)
                                .ThenInclude(hu => hu.House)
                            .Where(u => u.Id == id)
                            .SingleOrDefault();
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<IActionResult> CreateOrUpdateUserAsync([FromBody] User editUser)
        {
            if (editUser == null)
            {
                return BadRequest();
            }
            var user = _context.User.FirstOrDefault(u => u.Id == editUser.Id);

            if (user != null)
            {
                user.UpdateObjectFromOther(editUser);
            }
            else
            {
                // _context.User.Add(editUser);
                return NotFound();
            }

            _context.SaveChanges();

            return CreatedAtRoute("GetUser", new { id = editUser.Id }, editUser);
        }

        //[HttpPost]
        //public IActionResult CreateUser([FromBody] User user)
        //{
        //    if (user == null)
        //    {
        //        return BadRequest();
        //    }

        //    _context.User.Add(user);
        //    _context.SaveChanges();

        //    return CreatedAtRoute("GetUser", new { id = user.Id }, user);
        //}


    }
}