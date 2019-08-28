﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Mvc;
using InspectionReport.Models;
using Microsoft.AspNetCore.Authorization;

namespace InspectionReport.Controllers
{
    //[Authorize]
    [Route("api/User")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ReportContext _context;

        public UserController(ReportContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_context.User.ToList());
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
        public IActionResult CreateOrUpdateUser([FromBody] User editUser)
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
                _context.User.Add(editUser);
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