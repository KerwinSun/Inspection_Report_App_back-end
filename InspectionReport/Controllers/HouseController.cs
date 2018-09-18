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

        [HttpGet(Name = "GetAll")]
        public IActionResult GetAll()
        {
            ICollection<House> houses = _context.House
                                    .Include(h => h.Categories)
                                        .ThenInclude(c => c.Features)
                                    .ToList();            
            return Ok(houses);
        }


        [HttpGet("{id}", Name = "GetHouse")]
        public IActionResult GetById(long id)
        {
            House house = _context.House
                            .Where(h => h.Id == id)
                            .Include(h => h.Categories)
                                .ThenInclude(c => c.Features)
                            .Include(h => h.InspectedBy)
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

        [HttpPost]
        public IActionResult CreateOrUpdateHouse([FromBody] House house)
        {
            if (house == null)
            {
                return BadRequest();
            }

            House houseInContext = _context.House
                .Where(h => h.Id == house.Id)
                .Include(h => h.Categories)
                    .ThenInclude(c => c.Features)
                .Include(h => h.InspectedBy)
                .SingleOrDefault();

            if (houseInContext != null)
            {
                houseInContext.UpdateObjectFromOther(house);
            }
            else
            {
                //Add house-user relationship 
                foreach (HouseUser hu in house.InspectedBy ?? Enumerable.Empty<HouseUser>())
                {
                    hu.HouseId = house.Id;
                }

                _context.House.Add(house);
            }

            _context.SaveChanges();

            return CreatedAtRoute("GetHouse", new { id = house.Id }, house);

        }

        [HttpDelete("{id}")]
        public IActionResult Delete(long id)
        {
            House house = _context.House.Find(id);
            if (house == null)
            {
                return NotFound();
            }
            else
            {
                _context.Remove(house);
                _context.SaveChanges();
                return Ok();
            }

        }
    }
}