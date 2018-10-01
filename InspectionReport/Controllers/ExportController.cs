﻿using System.IO;
using System.Linq;
using DinkToPdf;
using DinkToPdf.Contracts;
using InspectionReport.Models;
using InspectionReport.Services.Interfaces;
using InspectionReport.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InspectionReport.Controllers
{
    [Authorize]
    [Route("api/Export")]
    public class ExportController : Controller
    {
        private readonly ReportContext _context;
        private readonly IConverter _converter;
        private readonly IAuthorizeService _authorizeService;

        public ExportController(ReportContext context, IConverter converter, IAuthorizeService authorizeService)
        {
            _context = context;
            _converter = converter;
            _authorizeService = authorizeService;
        }

        /// <summary>
        /// Generate the PDF report for a specific house.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>byte[] for the PDF report file</returns>
        private byte[] GeneratePDF(long id)
        {
            House house = _context.House
                .Where(h => h.Id == id)
                .Include(h => h.Categories)
                .ThenInclude(c => c.Features)
                .Include(h => h.InspectedBy)
                .SingleOrDefault();

            if (house == null)
            {
                return null;
            }

            string names = "";

            foreach (var hu in house.InspectedBy)
            {
                User user = _context.User
                    .Where(u => u.Id == hu.UserId)
                    .SingleOrDefault();

                if (names == "")
                {
                    names = names + user.Name;
                }
                else
                {
                    names = names + ", " + user.Name;
                }
            }

            GlobalSettings globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings {Top = 10},
                DocumentTitle = "Inspection PDF Report",
            };

            //TODO: 
            //I strongly recommend this to be created as a service and be injected via DI. (Victor)
            TemplateGenerator templateGenerator = new TemplateGenerator(_context, _authorizeService);


            ObjectSettings objectSettings = new ObjectSettings
            {
                PagesCount = true,


                HtmlContent = templateGenerator.Generate(house,
                    names),

                WebSettings =
                {
                    DefaultEncoding = "utf-8",
                    UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "styles.css")
                },
                HeaderSettings = {FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Line = true},
                FooterSettings = {FontName = "Arial", FontSize = 9, Line = true, Center = "Report Footer"}
            };

            HtmlToPdfDocument pdf = new HtmlToPdfDocument
            {
                GlobalSettings = globalSettings,
                Objects = {objectSettings}
            };

            byte[] file = _converter.Convert(pdf);

            return file;
        }

        /// <summary>
        /// Get the PDF report for a specific house.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>IActionResult for HTTP response</returns>
        [HttpGet("{id}", Name = "GetPDF")]
        public IActionResult GetPDF(long id)
        {
            byte[] file = this.GeneratePDF(id);

            return file == null ? (IActionResult) NotFound() : File(file, "application/pdf");
        }
    }
}