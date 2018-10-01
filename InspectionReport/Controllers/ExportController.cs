using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InspectionReport.Models;
using InspectionReport.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;

namespace InspectionReport.Controllers
{
	[Route("api/Export")]
	public class ExportController : Controller
	{
		private readonly ReportContext _context;
		private XFont _largeRegularFont;
		private XFont _normalRegularFont;
		private XFont _normalBoldFont;
		private PdfDocument _document;
		private const int initialY = 30;
		private const int initialX = 30;
		private int currentY = initialY;
		private const int lineSpace = 25;

		public ExportController(ReportContext context)
		{
			_context = context;

			GlobalFontSettings.FontResolver = new FontResolver();
			_largeRegularFont = new XFont("OpenSans", 20, XFontStyle.Bold);
			_normalRegularFont = new XFont("OpenSans", 13, XFontStyle.Regular);
			_normalBoldFont = new XFont("OpenSans", 13, XFontStyle.Bold);
			_document = new PdfDocument();
		}

		[HttpGet("{id}")]
		public IActionResult GeneratePDF(long id)
		{
			FileResult file = CreatePDF(id);

			return file ?? (IActionResult)NotFound();
		}

		private FileResult CreatePDF(long id)
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
				User user = _context.Users
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

			CreateTitlePage(house, names);
			CreateHousePages(house);
			CreateImagePages(house);

			string pdfFilename = "InspectionReport" + "" + ".pdf";

			using (MemoryStream ms = new MemoryStream())
			{
				_document.Save(ms, false);
				byte[] buffer = new byte[ms.Length];
				ms.Seek(0, SeekOrigin.Begin);
				ms.Flush();
				ms.Read(buffer, 0, (int)ms.Length);
				byte[] docBytes = ms.ToArray();
				return File(docBytes, "application/pdf", pdfFilename);
			}
		}

		private void CreateTitlePage(House house, string names)
		{
			PdfPage page = _document.AddPage();
			XGraphics gfx = XGraphics.FromPdfPage(page);
			gfx.DrawString("Hitch Building Inspections", _largeRegularFont, XBrushes.Blue, new XRect(0, 25, page.Width, page.Height), XStringFormats.TopCenter);
			currentY = 100;
			gfx.DrawString("Date of Inspection:", _normalBoldFont, XBrushes.Black, initialX, currentY, XStringFormats.Default);
			gfx.DrawString(house.InspectionDate.ToShortDateString(), _normalRegularFont, XBrushes.Black, initialX + 200, currentY, XStringFormats.Default);
			NewLine();
			gfx.DrawString("Client Information", _normalBoldFont, XBrushes.Black, initialX, currentY, XStringFormats.Default);
			NewLine();
			gfx.DrawString("Summonsed By:", _normalBoldFont, XBrushes.Black, initialX, currentY, XStringFormats.Default);
			gfx.DrawString("summonsed by", _normalRegularFont, XBrushes.Black, initialX + 200, currentY, XStringFormats.Default);
			NewLine();
			gfx.DrawString("Inspected By:", _normalBoldFont, XBrushes.Black, initialX, currentY, XStringFormats.Default);
			gfx.DrawString("inspected by", _normalRegularFont, XBrushes.Black, initialX + 200, currentY, XStringFormats.Default);
			NewLine();
			gfx.DrawString("Contact Details", _normalBoldFont, XBrushes.Black, initialX, currentY, XStringFormats.Default);
			NewLine();
			gfx.DrawString("Home ph #:", _normalBoldFont, XBrushes.Black, initialX, currentY, XStringFormats.Default);
			gfx.DrawString("home phone number", _normalRegularFont, XBrushes.Black, initialX + 200, currentY, XStringFormats.Default);
			NewLine();
			gfx.DrawString("Mobile #:", _normalBoldFont, XBrushes.Black, initialX, currentY, XStringFormats.Default);
			gfx.DrawString("mobile number", _normalRegularFont, XBrushes.Black, initialX + 200, currentY, XStringFormats.Default);
			NewLine();
			gfx.DrawString("Address:", _normalBoldFont, XBrushes.Black, initialX, currentY, XStringFormats.Default);
			gfx.DrawString("client address", _normalRegularFont, XBrushes.Black, initialX + 200, currentY, XStringFormats.Default);
			NewLine();
			gfx.DrawString("Email Address:", _normalBoldFont, XBrushes.Black, initialX, currentY, XStringFormats.Default);
			gfx.DrawString("email address", _normalRegularFont, XBrushes.Black, initialX + 200, currentY, XStringFormats.Default);
			NewLine();
			gfx.DrawString("Real Estate & Agent:", _normalBoldFont, XBrushes.Black, initialX, currentY, XStringFormats.Default);
			gfx.DrawString("real estate & agent", _normalRegularFont, XBrushes.Black, initialX + 200, currentY, XStringFormats.Default);
			NewLine();
			gfx.DrawString("House Description", _normalBoldFont, XBrushes.Black, initialX, currentY, XStringFormats.Default);
			gfx.DrawString("house description", _normalRegularFont, XBrushes.Black, initialX + 200, currentY, XStringFormats.Default);
			NewLine();
			gfx.DrawString("Estimate Summary:", _normalBoldFont, XBrushes.Black, initialX, currentY, XStringFormats.Default);
			gfx.DrawString("estimate summary", _normalRegularFont, XBrushes.Black, initialX + 200, currentY, XStringFormats.Default);
			NewLine();
			gfx.DrawString("Rooms Summary:", _normalBoldFont, XBrushes.Black, initialX, currentY, XStringFormats.Default);
			gfx.DrawString("rooms summary", _normalRegularFont, XBrushes.Black, initialX + 200, currentY, XStringFormats.Default);
			NewLine();
			gfx.DrawString("Construction Types:", _normalBoldFont, XBrushes.Black, initialX, currentY, XStringFormats.Default);
			gfx.DrawString("construction type", _normalRegularFont, XBrushes.Black, initialX + 200, currentY, XStringFormats.Default);
			NewLine();
		}

		private void CreateHousePages(House house)
		{
			PdfPage page = _document.AddPage();
			XGraphics gfx = XGraphics.FromPdfPage(page);
			gfx.DrawString("house to draw", _normalRegularFont, XBrushes.Black, initialX, initialY, XStringFormats.Default);
		}

		private void CreateImagePages(House house)
		{
			PdfPage page = _document.AddPage();
			XGraphics gfx = XGraphics.FromPdfPage(page);
			gfx.DrawString("Images", _normalRegularFont, XBrushes.Black, initialX, initialY, XStringFormats.Default);
		}

		private void NewLine()
		{
			currentY += lineSpace;
		}
	}
}
