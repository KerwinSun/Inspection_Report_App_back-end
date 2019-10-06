using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InspectionReport.Models;
using InspectionReport.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace InspectionReport.Controllers
{
    //[Authorize]
    [Route("api/Category")]
    public class CategoryController : Controller
    {

        private readonly ReportContext _context;
        private readonly IAuthorizeService _authorizeService;

        public CategoryController(ReportContext context, IAuthorizeService authorizeService)
        {
            _context = context;
            _authorizeService = authorizeService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var c = JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText("Template.js"));
            return Ok(c);
        }

    }

}
