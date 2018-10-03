using System;
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
using PdfSharp.Drawing.Layout;
using System.Net;

namespace InspectionReport.Controllers
{
	[Route("api/Export")]
	public class ExportController : Controller
	{
		private readonly ReportContext _context;
		private ImageHandler _imageHandler;
		private readonly IImageService _imageService;
		private PdfDocument _document;
		private PdfPage _page;
		private XGraphics _gfx;
		private XTextFormatter _tf;
		private readonly XFont _largeRegularFont;
		private XFont _normalRegularFont;
		private readonly XFont _normalBoldFont;
		private const int _initialY = 50;
		private const int _initialX = 50;
		private int _currentY = _initialY;
		private const int _lineSpace = 25;
		private readonly int _vMargin = 5;
		private readonly XSolidBrush _color;
		private int _pageNumber = 0;
		private readonly int _nameWidth = 180;
		private readonly int _commentsWidth = 240;
		private readonly int _labelWidth = 200;
		private readonly int _valueWidth = 300;

		public ExportController(ReportContext context, IImageService imageService)
		{
			_context = context;
			_imageService = imageService;
			_imageHandler = new ImageHandler();
			_largeRegularFont = new XFont("Arial", 20, XFontStyle.Bold);
			_normalRegularFont = new XFont("Arial", 13, XFontStyle.Regular);
			_normalBoldFont = new XFont("Arial", 13, XFontStyle.Bold);
			_document = new PdfDocument();
			_color = new XSolidBrush(XColor.FromArgb(179, 204, 204));
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

			CreateTitlePage(house, names);
			CreateHousePages(house);
			CreateCommentsPages(house);
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
			Client client = house.SummonsedBy;
			AddNewPage();
			_gfx.DrawString("Hitch Building Inspections", _largeRegularFont, XBrushes.Blue, new XRect(0, 25, _page.Width, _page.Height), XStringFormats.TopCenter);
			_currentY = 100;
			WriteLine("Date of Inspection: ", _normalBoldFont, _initialX);
			WriteLine(house.InspectionDate.ToShortDateString(), _normalRegularFont, _initialX + _labelWidth);
			NewLine();
			WriteLine("Client Information", _normalBoldFont, _initialX);
			NewLine();
			WriteLine("Summonsed By: ", _normalBoldFont, _initialX);
			WriteLine("summonsed by", _normalRegularFont, _initialX + _labelWidth);
			//WriteLine(client.Name, _normalRegularFont, _initialX + _labelWidth);
			NewLine();
			WriteLine("Inspected By: ", _normalBoldFont, _initialX);
			WriteLine(names, _normalRegularFont, _initialX + _labelWidth);
			NewLine();
			WriteLine("Contact Details", _normalBoldFont, _initialX);
			NewLine();
			WriteLine("Home ph #: ", _normalBoldFont, _initialX);
			WriteLine("home phone number", _normalRegularFont, _initialX + _labelWidth);
			//WriteLine(client.HomePhoneNumber, _normalRegularFont, _initialX + _labelWidth);
			NewLine();
			WriteLine("Mobile #: ", _normalBoldFont, _initialX);
			WriteLine("mobile number", _normalRegularFont, _initialX + _labelWidth);
			//WriteLine(client.MobilePhoneNumber, _normalRegularFont, _initialX + _labelWidth);
			NewLine();
			//WriteLine("Address: ", _normalBoldFont, _initialX);
			//WriteLine(client.Address, _normalRegularFont, _initialX + _labelWidth);
			//NewLine();
			WriteLine("Email Address: ", _normalBoldFont, _initialX);
			WriteLine("email address", _normalRegularFont, _initialX + _labelWidth);
			//WriteLine(client.EmailAddress, _normalRegularFont, _initialX + _labelWidth);
			NewLine();
			//WriteLine("Real Estate & Agent: ", _normalBoldFont, _initialX);
			//WriteLine(client.RealEstate, _normalRegularFont, _initialX + _labelWidth);
			//NewLine();
			WriteLine("House Description", _normalBoldFont, _initialX);
			NewLine();
			WriteLine("Estimate Summary: ", _normalBoldFont, _initialX);
			//WriteLine(house.EstimateSummary, _normalRegularFont, _initialX + _labelWidth, _valueWidth);
			NewLine();
			WriteLine("Rooms Summary: ", _normalBoldFont, _initialX);
			//WriteLine(house.RoomsSummary, _normalRegularFont, _initialX + _labelWidth, _valueWidth);
			NewLine();
			WriteLine("Construction Types: ", _normalBoldFont, _initialX);
			//WriteLine(house.ConstructionType, _normalRegularFont, _initialX + _labelWidth, _valueWidth);
			NewLine();

			foreach (Category category in house.Categories)
			{
				if (category.Name == "Overview")
				{
					foreach (Feature feature in category.Features)
					{
						if (feature.Name == "General")
						{
							List<string> URIResults = _imageService.GetUriResultsForFeature(feature.Id, out HttpStatusCode statusCode, HttpContext.User);
							if (URIResults != null && URIResults.Count != 0)
							{
								XImage image = _imageHandler.FromURI(URIResults[0].ToString());
								double scale = (image.PixelWidth / 450) >= 1 ? (image.PixelWidth / 450) : 1;
								_gfx.DrawImage(image, _initialX + 10, _currentY, image.PixelWidth / scale, image.PixelHeight / scale);
							}
						}
					}
					break;
				}
			}
		}

		private void CreateHousePages(House house)
		{
			AddNewHousePage();
			foreach (Category category in house.Categories)
			{
				if (category.Name != "Overview")
				{
					double titleHeight = _gfx.MeasureString(category.Name, _normalBoldFont).Height;
					if (_currentY + titleHeight * 2 > 750)
					{
						AddNewHousePage();
					}
					WriteCategory(category.Name, _normalBoldFont, _initialX);
					NewLine();
					foreach (Feature feature in category.Features)
					{
						int nameLineSpace = (int)Math.Ceiling(GetTextHeight(feature.Name, _nameWidth) - 1);
						int commentsLineSpace = (int)Math.Ceiling(GetTextHeight(feature.Comments, _commentsWidth) - 1);
						int largerLineSpace = nameLineSpace > commentsLineSpace ? nameLineSpace : commentsLineSpace;
						NewRow(largerLineSpace + 16, feature);
					}
				}
			}
		}

		private void CreateCommentsPages(House house)
		{
			AddNewPage();
			WriteCategory("General Comments", _normalBoldFont, _initialX);
			NewLine();
			//int commentsLineSpace = (int)Math.Ceiling(GetTextHeight(house.Comments, 500) - 1);
		}

		private void CreateImagePages(House house)
		{
			AddNewPage();
			WriteCategory("Images", _normalBoldFont, _initialX);
			NewLine();
			foreach (Category category in house.Categories)
			{
				foreach (Feature feature in category.Features)
				{
					List<string> URIResults = _imageService.GetUriResultsForFeature(feature.Id, out HttpStatusCode statusCode, HttpContext.User);
					if (URIResults != null && URIResults.Count != 0)
					{
						double titleHeight = _gfx.MeasureString(feature.Name, _normalBoldFont).Height;
						XImage image = _imageHandler.FromURI(URIResults[0].ToString());
						double scale = (image.PixelWidth / 450) >= 1 ? (image.PixelWidth / 450) : 1;
						int imageHeight = (int)Math.Ceiling(image.PixelHeight / scale);
						if (_currentY + titleHeight + imageHeight > 750)
						{
							AddNewPage();
						}
						WriteLine(category.Name + " - " + feature.Name, _normalBoldFont, _initialX);
					}
					if (statusCode != HttpStatusCode.OK)
					{
						//TODO: @CJ think about what happens when image cannot be get. 
						throw new NotImplementedException("Unexpected error, image cannot be found or is unauthorized.");
					}
					foreach (string URIResult in URIResults)
					{
						XImage image = _imageHandler.FromURI(URIResult.ToString());
						double scale = (image.PixelWidth / 450) >= 1 ? (image.PixelWidth / 450) : 1;
						int imageWidth = (int)Math.Ceiling(image.PixelWidth / scale);
						int imageHeight = (int)Math.Ceiling(image.PixelHeight / scale);
						NewImageRow(image, imageWidth, imageHeight + 10);
					}
				}
			}
		}

		private void WriteLine(string stringToWrite, XFont font, int x, int textWidth)
		{
			double height = _gfx.MeasureString(stringToWrite, font).Height;
			_tf.DrawString(stringToWrite, font, XBrushes.Black, new XRect(x, _currentY - height / 2, textWidth, _page.Height), XStringFormats.TopLeft);
		}

		private void WriteLine(string stringToWrite, XFont font, int x)
		{
			_gfx.DrawString(stringToWrite, font, XBrushes.Black, x, _currentY + _vMargin);
		}

		private void WriteCategory(string stringToWrite, XFont font, int x)
		{
			_gfx.DrawRectangle(_color, _initialX, _currentY - 15, _page.Width - 2 * _initialX + 5, 25);
			_gfx.DrawString(stringToWrite, font, XBrushes.Black, x, _currentY);
		}

		private void AddNewHousePage()
		{
			_currentY = _initialY;
			_page = _document.AddPage();
			_pageNumber++;
			_gfx = XGraphics.FromPdfPage(_page);
			AddFooter();
			_gfx.DrawString(_pageNumber.ToString(), _normalBoldFont, XBrushes.Black, 535, 795);
			_tf = new XTextFormatter(_gfx);
			WriteLine("A", _normalBoldFont, _initialX + 420);
			WriteLine("B", _normalBoldFont, _initialX + 450);
			WriteLine("C", _normalBoldFont, _initialX + 480);
			NewLine();
		}

		private void AddNewPage()
		{
			_currentY = _initialY;
			_page = _document.AddPage();
			_pageNumber++;
			_gfx = XGraphics.FromPdfPage(_page);
			_gfx.DrawString(_pageNumber.ToString(), _normalBoldFont, XBrushes.Black, 535, 795);
			_tf = new XTextFormatter(_gfx);
		}

		private void AddFooter()
		{
			_gfx.DrawString("A - Good", _normalBoldFont, XBrushes.Black, 70, 765);
			_gfx.DrawString("B - Will need attention soon", _normalBoldFont, XBrushes.Black, 70, 780);
			_gfx.DrawString("C - Need immediate attention", _normalBoldFont, XBrushes.Black, 70, 795);
			_gfx.DrawRectangle(new XPen(XColors.Black), _initialX, 750, 500, 50);
		}

		private void NewLine()
		{
			_currentY += _lineSpace;
		}

		private void NewRow(int height, Feature feature)
		{
			if (_currentY + height > 750)
			{
				AddNewHousePage();
			}
			WriteLine(feature.Name, _normalRegularFont, _initialX, _nameWidth);
			WriteLine(feature.Comments, _normalRegularFont, _initialX + _nameWidth, _commentsWidth);
			if (feature.Grade == 1)
			{
				WriteLine("X", _normalRegularFont, _initialX + 420);
			}
			else if (feature.Grade == 2)
			{
				WriteLine("X", _normalRegularFont, _initialX + 450);
			}
			else if (feature.Grade == 3)
			{
				WriteLine("X", _normalRegularFont, _initialX + 480);
			}
			_gfx.DrawRectangle(new XPen(XColors.LightGray), _initialX, _currentY - 15, 410, height);
			_gfx.DrawRectangle(new XPen(XColors.Black), _initialX + 410, _currentY - 15, 30, height);
			_gfx.DrawRectangle(new XPen(XColors.Black), _initialX + 440, _currentY - 15, 30, height);
			_gfx.DrawRectangle(new XPen(XColors.Black), _initialX + 470, _currentY - 15, 30, height);
			_currentY += height;
		}

		private void NewImageRow(XImage image, int width, int height)
		{
			if (_currentY + height > 750)
			{
				AddNewPage();
			}
			_gfx.DrawImage(image, _initialX + 10, _currentY + 10, width, height);
			_currentY += height + 30;
		}

		private double GetTextHeight(string text, double rectWidth)
		{
			double fontHeight = _normalRegularFont.GetHeight();
			XSize measure = _gfx.MeasureString(text, _normalRegularFont);
			double absoluteTextHeight = measure.Height;
			double absoluteTextWidth = measure.Width;

			if (absoluteTextWidth > rectWidth)
			{
				var linesToAdd = (int)Math.Ceiling(absoluteTextWidth / 100) - 1;
				return absoluteTextHeight + linesToAdd * (fontHeight);
			}
			return absoluteTextHeight;
		}
	}
}
