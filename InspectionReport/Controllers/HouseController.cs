﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InspectionReport.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace InspectionReport.Controllers
{
    [Route("api/House")]
    public class HouseController : Controller
    {
        private readonly ReportContext _context;

        public HouseController(ReportContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_context.House.ToList());
        }


        // GET api/<controller>/5
        [HttpGet("{id}", Name = "GetHouse")]
        public IActionResult GetById(long id)
        {
            //House house = _context.House.Find(id);
            House house = _context.House
                            .Where(h => h.Id == id)
                            .Include(h => h.Categories)
                                .ThenInclude(c => c.Features)
                            .SingleOrDefault();


            if (house == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(house);
            }
        }

        // POST api/<controller>
        [HttpPost]
        public IActionResult CreateHouse([FromBody] House house)
        {
            if (house == null)
            {
                return BadRequest();
            }

            House check = _context.House.Find(house.Id);

            if (check != null)
            {
                /*
                 Note:
                 https://stackoverflow.com/questions/37586659/replace-entity-in-context-with-a-different-instance-of-the-same-entity
                 */
                _context.Entry(check).State = EntityState.Detached;
                _context.House.Attach(house);
                _context.Entry(house).State = EntityState.Modified;
            }
            else
            {
                _context.House.Add(house);
            }
            _context.SaveChanges();

            return CreatedAtRoute("GetHouse", new { id = house.Id }, house);

        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(long id)
        {
            House house = _context.House.Find(id);
            if (house == null)
            {
                return NotFound();
            } else
            {
                _context.Remove(house);
                _context.SaveChanges();
                return Ok();
            }
            
        }
    }
}
