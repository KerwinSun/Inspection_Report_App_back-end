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
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace InspectionReport.Controllers
{
	[Route("api/Export")]
	public class ExportController : Controller
	{
		private readonly ReportContext _context;
		private ImageHandler _imageHandler;
		private XFont _largeRegularFont;
		private XFont _normalRegularFont;
		private XFont _normalBoldFont;
		private PdfDocument _document;
		private const int initialY = 50;
		private const int initialX = 50;
		private int currentY = initialY;
		private const int lineSpace = 25;

		public ExportController(ReportContext context)
		{
			_context = context;
			_imageHandler = new ImageHandler();
			_largeRegularFont = new XFont("Arial", 20, XFontStyle.Bold);
			_normalRegularFont = new XFont("Arial", 13, XFontStyle.Regular);
			_normalBoldFont = new XFont("Arial", 13, XFontStyle.Bold);
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

			string pdfFilename = "InspectionReport" + house.Id + ".pdf";

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
			gfx.DrawString("Date of Inspection:", _normalBoldFont, XBrushes.Black, initialX, currentY);
			gfx.DrawString(house.InspectionDate.ToShortDateString(), _normalRegularFont, XBrushes.Black, initialX + 200, currentY);
			NewLine();
			gfx.DrawString("Client Information", _normalBoldFont, XBrushes.Black, initialX, currentY);
			NewLine();
			gfx.DrawString("Summonsed By:", _normalBoldFont, XBrushes.Black, initialX, currentY);
			gfx.DrawString("summonsed by", _normalRegularFont, XBrushes.Black, initialX + 200, currentY);
			NewLine();
			gfx.DrawString("Inspected By:", _normalBoldFont, XBrushes.Black, initialX, currentY);
			gfx.DrawString("inspected by", _normalRegularFont, XBrushes.Black, initialX + 200, currentY);
			NewLine();
			gfx.DrawString("Contact Details", _normalBoldFont, XBrushes.Black, initialX, currentY);
			NewLine();
			gfx.DrawString("Home ph #:", _normalBoldFont, XBrushes.Black, initialX, currentY);
			gfx.DrawString("home phone number", _normalRegularFont, XBrushes.Black, initialX + 200, currentY);
			NewLine();
			gfx.DrawString("Mobile #:", _normalBoldFont, XBrushes.Black, initialX, currentY);
			gfx.DrawString("mobile number", _normalRegularFont, XBrushes.Black, initialX + 200, currentY);
			NewLine();
			gfx.DrawString("Address:", _normalBoldFont, XBrushes.Black, initialX, currentY);
			gfx.DrawString("client address", _normalRegularFont, XBrushes.Black, initialX + 200, currentY);
			NewLine();
			gfx.DrawString("Email Address:", _normalBoldFont, XBrushes.Black, initialX, currentY);
			gfx.DrawString("email address", _normalRegularFont, XBrushes.Black, initialX + 200, currentY);
			NewLine();
			gfx.DrawString("Real Estate & Agent:", _normalBoldFont, XBrushes.Black, initialX, currentY);
			gfx.DrawString("real estate & agent", _normalRegularFont, XBrushes.Black, initialX + 200, currentY);
			NewLine();
			gfx.DrawString("House Description", _normalBoldFont, XBrushes.Black, initialX, currentY);
			gfx.DrawString("house description", _normalRegularFont, XBrushes.Black, initialX + 200, currentY);
			NewLine();
			gfx.DrawString("Estimate Summary:", _normalBoldFont, XBrushes.Black, initialX, currentY);
			gfx.DrawString("estimate summary", _normalRegularFont, XBrushes.Black, initialX + 200, currentY);
			NewLine();
			gfx.DrawString("Rooms Summary:", _normalBoldFont, XBrushes.Black, initialX, currentY);
			gfx.DrawString("rooms summary", _normalRegularFont, XBrushes.Black, initialX + 200, currentY);
			NewLine();
			gfx.DrawString("Construction Types:", _normalBoldFont, XBrushes.Black, initialX, currentY);
			gfx.DrawString("construction type", _normalRegularFont, XBrushes.Black, initialX + 200, currentY);
			NewLine();
			//XImage image = _imageHandler.FromURI(house.categories[0].);
			XImage image = _imageHandler.FromURI("https://camo.githubusercontent.com/556a7850bef41de27438eeebc4c1acbdc494d9c5/68747470733a2f2f692e696d6775722e636f6d2f687a3863486e712e706e67");
			double scale = (image.PixelWidth / 450) >= 1 ? (image.PixelWidth / 450) : 1;
			gfx.DrawImage(image, initialX + 10, currentY, image.PixelWidth / scale, image.PixelHeight / scale);
		}

		private void CreateHousePages(House house)
		{
			PdfPage page = _document.AddPage();
			XGraphics gfx = XGraphics.FromPdfPage(page);
			currentY = 50;

			foreach (Category category in house.Categories)
			{
				if (currentY < 450)
				{

				}
				gfx.DrawString(category.Name, _normalBoldFont, XBrushes.Black, initialX, currentY);
				gfx.DrawString("Count: " + category.Count.ToString(), _normalBoldFont, XBrushes.Black, initialX + 300, currentY);
				NewLine();
				DrawFeatureTable(gfx, category);
			}
		}

		private void DrawFeatureTable(XGraphics gfx, Category category)
		{
			gfx.DrawString("Name", _normalBoldFont, XBrushes.Black, initialX, currentY);
			gfx.DrawString("Comments", _normalBoldFont, XBrushes.Black, initialX + 100, currentY);
			gfx.DrawString("A", _normalBoldFont, XBrushes.Black, initialX + 400, currentY);
			gfx.DrawString("B", _normalBoldFont, XBrushes.Black, initialX + 420, currentY);
			gfx.DrawString("C", _normalBoldFont, XBrushes.Black, initialX + 440, currentY);
			NewLine();

			foreach (Feature feature in category.Features)
			{
				gfx.DrawString(feature.Name, _normalRegularFont, XBrushes.Black, initialX, currentY);
				gfx.DrawString(feature.Comments, _normalRegularFont, XBrushes.Black, initialX + 100, currentY);
				if (feature.Grade == 1)
				{
					gfx.DrawString("X", _normalRegularFont, XBrushes.Black, initialX + 400, currentY);
				}
				else if (feature.Grade == 2)
				{
					gfx.DrawString("X", _normalRegularFont, XBrushes.Black, initialX + 420, currentY);
				}
				else if (feature.Grade == 3)
				{
					gfx.DrawString("X", _normalRegularFont, XBrushes.Black, initialX + 440, currentY);
				}
				NewLine();
			}
		}

		private void CreateImagePages(House house)
		{
			PdfPage page = _document.AddPage();
			XGraphics gfx = XGraphics.FromPdfPage(page);
			gfx.DrawString("Images", _normalRegularFont, XBrushes.Black, initialX, initialY);
		}

		private void NewLine()
		{
			currentY += lineSpace;
		}
	}
}
