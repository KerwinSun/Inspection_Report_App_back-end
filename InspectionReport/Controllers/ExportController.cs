﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InspectionReport.Models;
using InspectionReport.Services.Interfaces;
using InspectionReport.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
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
		private ImageController _iController;
		private ImageHandler _imageHandler;
		private XFont _largeRegularFont;
		private XFont _normalRegularFont;
		private XFont _normalBoldFont;
		private PdfDocument _document;
		private PdfPage _page;
		private XGraphics _gfx;
		private const int initialY = 50;
		private const int initialX = 50;
		private int currentY = initialY;
		private const int lineSpace = 25;
		private readonly IAuthorizeService _authorizeService;

		public ExportController(ReportContext context, IAuthorizeService authorizeService)
		{
			_context = context;
			_authorizeService = authorizeService;
			_iController = new ImageController(_context, authorizeService);
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
			_page = _document.AddPage();
			_gfx = XGraphics.FromPdfPage(_page);
			_gfx.DrawString("Hitch Building Inspections", _largeRegularFont, XBrushes.Blue, new XRect(0, 25, _page.Width, _page.Height), XStringFormats.TopCenter);
			currentY = 100;
			WriteLine("Date of Inspection: ", _normalBoldFont, initialX);
			WriteLine(house.InspectionDate.ToShortDateString(), _normalRegularFont, initialX + 200);
			NewLine();
			WriteLine("Client Information", _normalBoldFont, initialX);
			NewLine();
			WriteLine("Summonsed By: ", _normalBoldFont, initialX);
			WriteLine("summonsed by", _normalRegularFont, initialX + 200);
			NewLine();
			WriteLine("Inspected By: ", _normalBoldFont, initialX);
			WriteLine(names, _normalRegularFont, initialX + 200);
			NewLine();
			WriteLine("Contact Details", _normalBoldFont, initialX);
			NewLine();
			WriteLine("Home ph #: ", _normalBoldFont, initialX);
			WriteLine("home phone number", _normalRegularFont, initialX + 200);
			NewLine();
			WriteLine("Mobile #: ", _normalBoldFont, initialX);
			WriteLine("mobile number", _normalRegularFont, initialX + 200);
			NewLine();
			WriteLine("Address: ", _normalBoldFont, initialX);
			WriteLine("client address", _normalRegularFont, initialX + 200);
			NewLine();
			WriteLine("Email Address: ", _normalBoldFont, initialX);
			WriteLine("email address", _normalRegularFont, initialX + 200);
			NewLine();
			WriteLine("Real Estate & Agent: ", _normalBoldFont, initialX);
			WriteLine("real estate & agent", _normalRegularFont, initialX + 200);
			NewLine();
			WriteLine("House Description", _normalBoldFont, initialX);
			NewLine();
			WriteLine("Estimate Summary: ", _normalBoldFont, initialX);
			WriteLine("estimate summary", _normalRegularFont, initialX + 200);
			NewLine();
			WriteLine("Rooms Summary: ", _normalBoldFont, initialX);
			WriteLine("rooms summary", _normalRegularFont, initialX + 200);
			NewLine();
			WriteLine("Construction Types: ", _normalBoldFont, initialX);
			WriteLine("construction type", _normalRegularFont, initialX + 200);
			NewLine();
			//XImage image = _imageHandler.FromURI(house.categories[0].);
			XImage image = _imageHandler.FromURI("https://camo.githubusercontent.com/556a7850bef41de27438eeebc4c1acbdc494d9c5/68747470733a2f2f692e696d6775722e636f6d2f687a3863486e712e706e67");
			double scale = (image.PixelWidth / 450) >= 1 ? (image.PixelWidth / 450) : 1;
			_gfx.DrawImage(image, initialX + 10, currentY, image.PixelWidth / scale, image.PixelHeight / scale);
		}

		private void CreateHousePages(House house)
		{
			_page = _document.AddPage();
			_gfx = XGraphics.FromPdfPage(_page);
			_gfx.DrawLine(XPens.Black, 460, initialY, 460, _page.Height - initialY);
			_gfx.DrawLine(XPens.Black, 490, initialY, 490, _page.Height - initialY);
			_gfx.DrawLine(XPens.Black, 520, initialY, 520, _page.Height - initialY);
			_gfx.DrawLine(XPens.Black, 550, initialY, 550, _page.Height - initialY);
			currentY = 50;

			foreach (Category category in house.Categories)
			{
				WriteLine(category.Name, _normalBoldFont, initialX);
				WriteLine("Count: " + category.Count.ToString(), _normalBoldFont, initialX + 300);
				_gfx.DrawLine(XPens.Black, 460, currentY - 15, 460, currentY + 35);
				_gfx.DrawLine(XPens.Black, 490, currentY - 15, 490, currentY + 35);
				_gfx.DrawLine(XPens.Black, 520, currentY - 15, 520, currentY + 35);
				_gfx.DrawLine(XPens.Black, 550, currentY - 15, 550, currentY + 35);
				_gfx.DrawLine(XPens.Black, 460, currentY - 15, 550, currentY - 15);
				_gfx.DrawLine(XPens.Black, 460, currentY + 10, 550, currentY + 10);
				NewLine();
				DrawFeatures(_gfx, category);
			}
		}

		private void DrawFeatures(XGraphics _gfx, Category category)
		{
			WriteLine("Name", _normalBoldFont, initialX);
			WriteLine("Comments", _normalBoldFont, initialX + 200);
			WriteLine("A", _normalBoldFont, initialX + 420);
			WriteLine("B", _normalBoldFont, initialX + 450);
			WriteLine("C", _normalBoldFont, initialX + 480);
			_gfx.DrawLine(XPens.Black, 460, currentY - 15, 460, currentY + 35);
			_gfx.DrawLine(XPens.Black, 490, currentY - 15, 490, currentY + 35);
			_gfx.DrawLine(XPens.Black, 520, currentY - 15, 520, currentY + 35);
			_gfx.DrawLine(XPens.Black, 550, currentY - 15, 550, currentY + 35);
			_gfx.DrawLine(XPens.Black, 460, currentY - 15, 550, currentY - 15);
			_gfx.DrawLine(XPens.Black, 460, currentY + 10, 550, currentY + 10);
			NewLine();

			foreach (Feature feature in category.Features)
			{
				WriteLine(feature.Name, _normalRegularFont, initialX);
				WriteLine(feature.Comments, _normalRegularFont, initialX + 200);
				if (feature.Grade == 1)
				{
					WriteLine("X", _normalRegularFont, initialX + 420);
				}
				else if (feature.Grade == 2)
				{
					WriteLine("X", _normalRegularFont, initialX + 450);
				}
				else if (feature.Grade == 3)
				{
					WriteLine("X", _normalRegularFont, initialX + 480);
				}
				_gfx.DrawLine(XPens.Black, 460, currentY + 10, 460, currentY + 35);
				_gfx.DrawLine(XPens.Black, 490, currentY + 10, 490, currentY + 35);
				_gfx.DrawLine(XPens.Black, 520, currentY + 10, 520, currentY + 35);
				_gfx.DrawLine(XPens.Black, 550, currentY + 10, 550, currentY + 35);
				_gfx.DrawLine(XPens.Black, 460, currentY + 10, 550, currentY + 10);
				NewLine();
			}
		}

		private void CreateImagePages(House house)
		{
			_page = _document.AddPage();
			_gfx = XGraphics.FromPdfPage(_page);
			currentY = 50;
			WriteLine("Images", _normalBoldFont, initialX);
			NewLine();

			foreach(Category category in house.Categories)
			{
				foreach (Feature feature in category.Features)
				{
					List<Media> medias = _context.Media
						.Where(m => m.Feature == feature).ToList();
					if (medias != null && medias.Count != 0)
					{
						WriteLine(category.Name + " - " + feature.Name, _normalRegularFont, initialX);
					}
					if (_iController.GetImage(feature.Id).Result is OkObjectResult mediaQueryResult)
					{
						List<string> URIResults = mediaQueryResult.Value as List<string>;
						foreach (string URIResult in URIResults)
						{
							XImage image = _imageHandler.FromURI(URIResult.ToString());
							double scale = (image.PixelWidth / 450) >= 1 ? (image.PixelWidth / 450) : 1;
							_gfx.DrawImage(image, initialX + 10, currentY + 10, image.PixelWidth / scale, image.PixelHeight / scale);
						}
					}
				}
			}
		}

		private void WriteLine(string stringToWrite, XFont font, int x)
		{
			if (currentY < 700)
			{
				_gfx.DrawString(stringToWrite, font, XBrushes.Black, x, currentY);
			}
			else
			{
				currentY = initialY;
				_page = _document.AddPage();
				_gfx = XGraphics.FromPdfPage(_page);
				_gfx.DrawLine(XPens.Black, 460, initialY, 460, _page.Height - initialY);
				_gfx.DrawLine(XPens.Black, 490, initialY, 490, _page.Height - initialY);
				_gfx.DrawLine(XPens.Black, 520, initialY, 520, _page.Height - initialY);
				_gfx.DrawLine(XPens.Black, 550, initialY, 550, _page.Height - initialY);

				_gfx.DrawString(stringToWrite, font, XBrushes.Black, x, currentY);
			}
			//_gfx.DrawLine(XPens.Silver, initialX, currentY + 10, _page.Width - initialX, currentY + 10);
		}

		private void AddFooter()
		{

		}

		private void NewLine()
		{
			currentY += lineSpace;
		}
	}
}
