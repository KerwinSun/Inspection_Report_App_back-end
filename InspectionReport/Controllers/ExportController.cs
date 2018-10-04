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
using PdfSharp.Fonts;

namespace InspectionReport.Controllers
{
	//[Authorize]
	[Route("api/Export")]
	public class ExportController : Controller
	{
		private readonly ReportContext _context;
		private ImageHandler _imageHandler;
		private readonly IImageService _imageService;
		private readonly IAuthorizeService _authorizeService;
		private PdfDocument _document;
		private PdfPage _page;
		private XGraphics _gfx;
		private XTextFormatter _tf;
		private readonly XFont _largeRegularFont;
		private readonly XFont _medRegularFont;
		private readonly XFont _medBoldFont;
		private readonly XFont _smallBoldFont;
		private readonly XFont _smallRegularFont;
		private const int _initialY = 50;
		private const int _initialX = 50;
		private int _currentY = _initialY;
		private const int _lineSpace = 25;
		private readonly int _vMargin = 5;
		private readonly XSolidBrush _color;
		private int _pageNumber = 0;
		private readonly int _nameWidth = 180;
		private readonly int _commentsWidth = 230;
		private readonly int _labelWidth = 200;
		private readonly int _valueWidth = 300;
		private ReportText _text;

		public ExportController(ReportContext context, IAuthorizeService authorizeService, IImageService imageService)
		{
			_context = context;
			_imageService = imageService;
			_authorizeService = authorizeService;
			_imageHandler = new ImageHandler();
			_largeRegularFont = new XFont("Arial", 20, XFontStyle.Bold);
			_medRegularFont = new XFont("Arial", 13, XFontStyle.Regular);
			_medBoldFont = new XFont("Arial", 13, XFontStyle.Bold);
			_smallRegularFont = new XFont("Arial", 8, XFontStyle.Regular);
			_smallBoldFont = new XFont("Arial", 8, XFontStyle.Bold);
			_document = new PdfDocument();
			_color = new XSolidBrush(XColor.FromArgb(179, 204, 204));
			_text = new ReportText();
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
			CreateStatementOfPolicyPage();
			CreateCertificatePage(house);

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
			AddNewPage();
			_gfx.DrawString("Hitch Building Inspections", _largeRegularFont, XBrushes.Blue, new XRect(0, 25, _page.Width, _page.Height), XStringFormats.TopCenter);
			_currentY = 100;
			WriteLine("Date of Inspection: ", _medBoldFont, _initialX);
			WriteLine(house.InspectionDate.ToShortDateString(), _medRegularFont, _initialX + _labelWidth);
			NewLine();
			WriteLine("Inspected By: ", _medBoldFont, _initialX);
			WriteLine(names, _medRegularFont, _initialX + _labelWidth);
			NewLine();
			Client client = house.SummonsedBy;
			if (client != null)
			{
				WriteLine("Client Information", _medBoldFont, _initialX);
				NewLine();
				WriteLine("Summonsed By: ", _medBoldFont, _initialX);
				if (client.Name != null)
				{
					WriteLine(client.Name, _medRegularFont, _initialX + _labelWidth);
				}
				NewLine();
				WriteLine("Contact Details", _medBoldFont, _initialX);
				NewLine();
				WriteLine("Home ph #: ", _medBoldFont, _initialX);
				if (client.HomePhoneNumber != null)
				{
					WriteLine(client.HomePhoneNumber, _medRegularFont, _initialX + _labelWidth);
				}
				NewLine();
				WriteLine("Mobile #: ", _medBoldFont, _initialX);
				if (client.MobilePhoneNumber != null)
				{
					WriteLine("mobile number", _medRegularFont, _initialX + _labelWidth);
				}
				NewLine();
				//WriteLine("Address: ", _normalBoldFont, _initialX);
				//WriteLine(client.Address, _medRegularFont, _initialX + _labelWidth);
				//NewLine();
				WriteLine("Email Address: ", _medBoldFont, _initialX);
				if (client.EmailAddress != null)
				{
					WriteLine(client.EmailAddress, _medRegularFont, _initialX + _labelWidth);
				}
				NewLine();
				//WriteLine("Real Estate & Agent: ", _normalBoldFont, _initialX);
				//WriteLine(client.RealEstate, _normalRegularFont, _initialX + _labelWidth);
				//NewLine();
				WriteLine("House Description", _medBoldFont, _initialX);
				NewLine();
				WriteLine("Estimate Summary: ", _medBoldFont, _initialX);
				if (house.EstimateSummary != null)
				{
					WriteLine(house.EstimateSummary, _medRegularFont, _initialX + _labelWidth, _valueWidth);
				}
				NewLine();
				WriteLine("Rooms Summary: ", _medBoldFont, _initialX);
				if (house.RoomsSummary != null)
				{
					WriteLine(house.RoomsSummary, _medRegularFont, _initialX + _labelWidth, _valueWidth);
				}
				NewLine();
				WriteLine("Construction Types: ", _medBoldFont, _initialX);
				if (house.ConstructionType != null)
				{
					WriteLine(house.ConstructionType, _medRegularFont, _initialX + _labelWidth, _valueWidth);
				}
				NewLine();
			}
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
								double scale = (image.PixelWidth / 500) >= 1 ? (image.PixelWidth / 500) : 1;
								_gfx.DrawImage(image, _initialX, _currentY, image.PixelWidth / scale, image.PixelHeight / scale);
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
					double titleHeight = _gfx.MeasureString(category.Name, _medBoldFont).Height;
					if (_currentY + titleHeight * 2 > 730)
					{
						AddNewHousePage();
					}
					WriteCategory(category.Name, _medBoldFont, _initialX);
					NewLine();
					foreach (Feature feature in category.Features)
					{
						int nameLineSpace = (int)Math.Ceiling(GetTextHeight(feature.Name, _medRegularFont, _nameWidth) - 1);
						int commentsLineSpace = (int)Math.Ceiling(GetTextHeight(feature.Comments, _medRegularFont, _commentsWidth) - 1);
						int largerLineSpace = nameLineSpace > commentsLineSpace ? nameLineSpace : commentsLineSpace;
						NewRow(largerLineSpace + 16, feature);
					}
				}
			}
		}

		private void CreateCommentsPages(House house)
		{
			AddNewPage();
			WriteCategory("General Comments", _medBoldFont, _initialX);
			NewLine();
			string comments = "";
			if (house.Comments != null)
			{
				comments = house.Comments;
			}
			string[] commentsArray = comments.Split("\n\n");
			foreach (string comment in commentsArray)
			{
				string trimmedComment = comment.Trim();
				int commentsLineSpace = (int)Math.Ceiling(GetTextHeight(trimmedComment, _medRegularFont, 500) - 1);
				NewCommentRow(trimmedComment, commentsLineSpace);
			}
		}

		private void CreateImagePages(House house)
		{
			AddNewPage();
			WriteCategory("Images", _medBoldFont, _initialX);
			NewLine();
			foreach (Category category in house.Categories)
			{
				if (category.Name != "Overview")
				{
					foreach (Feature feature in category.Features)
					{
						if (feature.Name != "General")
						{
							List<string> URIResults = _imageService.GetUriResultsForFeature(feature.Id, out HttpStatusCode statusCode, HttpContext.User);
							if (URIResults != null && URIResults.Count != 0)
							{
								double titleHeight = _gfx.MeasureString(feature.Name, _medBoldFont).Height;
								XImage image = _imageHandler.FromURI(URIResults[0].ToString());
								double scale = (image.PixelWidth / 500) >= 1 ? (image.PixelWidth / 500) : 1;
								int imageHeight = (int)Math.Ceiling(image.PixelHeight / scale);
								if (_currentY + titleHeight + imageHeight > 750)
								{
									AddNewPage();
								}
								WriteLine(category.Name + " - " + feature.Name, _medBoldFont, _initialX);
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
			}
		}

		private void CreateStatementOfPolicyPage()
		{
			AddNewPage();
			_gfx.DrawString("Hitch Building Inspections", _largeRegularFont, XBrushes.Blue, new XRect(0, 25, _page.Width, _page.Height), XStringFormats.TopCenter);
			_currentY = 80;
			string policyText = _text.statementOfPolicy;
			string[] policyArray = policyText.Split("\r\n\r\n");
			foreach (string policy in policyArray)
			{
				int policyLineSpace = (int)Math.Ceiling(GetTextHeight(policy, _smallRegularFont, 500) - 1);
				NewPolicyRow(policy, policyLineSpace);
			}
		}

		private void CreateCertificatePage(House house)
		{

		}

		private void WriteLine(string stringToWrite, XFont font, int x, int textWidth)
		{
			double height = _gfx.MeasureString(stringToWrite, font).Height;
			_tf.DrawString(stringToWrite, font, XBrushes.Black, new XRect(x, _currentY - height / 2, textWidth, _page.Height), XStringFormats.TopLeft);
		}

		private void WriteLine(string stringToWrite, XFont font, int x, int textWidth, int textHeight)
		{
			_tf.DrawString(stringToWrite, font, XBrushes.Black, new XRect(x, _currentY, textWidth, textHeight), XStringFormats.TopLeft);
		}

		private void WriteLine(string stringToWrite, XFont font, int x)
		{
			_gfx.DrawString(stringToWrite, font, XBrushes.Black, x, _currentY + _vMargin);
		}

		private void WriteCategory(string stringToWrite, XFont font, int x)
		{
			_gfx.DrawRectangle(_color, _initialX, _currentY - 15, 500, 25);
			_gfx.DrawString(stringToWrite, font, XBrushes.Black, x, _currentY);
		}

		private void AddNewHousePage()
		{
			_currentY = _initialY;
			_page = _document.AddPage();
			_pageNumber++;
			_gfx = XGraphics.FromPdfPage(_page);
			AddFooter();
			_gfx.DrawString(_pageNumber.ToString(), _medBoldFont, XBrushes.Black, 535, 775);
			_tf = new XTextFormatter(_gfx);
			WriteLine("A", _medBoldFont, _initialX + 420);
			WriteLine("B", _medBoldFont, _initialX + 450);
			WriteLine("C", _medBoldFont, _initialX + 480);
			NewLine();
		}

		private void AddNewPage()
		{
			_currentY = _initialY;
			_page = _document.AddPage();
			_pageNumber++;
			_gfx = XGraphics.FromPdfPage(_page);
			_gfx.DrawString(_pageNumber.ToString(), _medBoldFont, XBrushes.Black, 535, 775);
			_tf = new XTextFormatter(_gfx);
		}

		private void AddFooter()
		{
			_gfx.DrawString("A - Good", _medBoldFont, XBrushes.Black, 70, 745);
			_gfx.DrawString("B - Will need attention soon", _medBoldFont, XBrushes.Black, 70, 760);
			_gfx.DrawString("C - Need immediate attention", _medBoldFont, XBrushes.Black, 70, 775);
			_gfx.DrawRectangle(new XPen(XColors.Black), _initialX, 730, 500, 50);
		}

		private void NewLine()
		{
			_currentY += _lineSpace;
		}

		private void NewRow(int height, Feature feature)
		{
			if (_currentY + height > 720)
			{
				AddNewHousePage();
			}
			WriteLine(feature.Name, _medRegularFont, _initialX, _nameWidth);
			WriteLine(feature.Comments, _medRegularFont, _initialX + _nameWidth, _commentsWidth);
			if (feature.Grade == 1)
			{
				WriteLine("X", _medRegularFont, _initialX + 420);
			}
			else if (feature.Grade == 2)
			{
				WriteLine("X", _medRegularFont, _initialX + 450);
			}
			else if (feature.Grade == 3)
			{
				WriteLine("X", _medRegularFont, _initialX + 480);
			}
			_gfx.DrawRectangle(new XPen(XColors.LightGray), _initialX, _currentY - 15, _nameWidth, height);
			_gfx.DrawRectangle(new XPen(XColors.LightGray), _nameWidth + _initialX, _currentY - 15, _commentsWidth, height);
			_gfx.DrawRectangle(new XPen(XColors.Black), _initialX + 410, _currentY - 15, 30, height);
			_gfx.DrawRectangle(new XPen(XColors.Black), _initialX + 440, _currentY - 15, 30, height);
			_gfx.DrawRectangle(new XPen(XColors.Black), _initialX + 470, _currentY - 15, 30, height);
			_currentY += height;
		}

		private void NewImageRow(XImage image, int width, int height)
		{
			if (_currentY + height > 720)
			{
				AddNewPage();
			}
			_gfx.DrawImage(image, _initialX + 10, _currentY + 10, width, height);
			_currentY += height + 30;
		}

		private void NewCommentRow(string text, int height)
		{
			if (_currentY + height > 720)
			{
				AddNewPage();
			}
			WriteLine(text, _medRegularFont, _initialX, 500);
			_gfx.DrawRectangle(new XPen(XColors.Black), _initialX, _currentY -15, 500, height + 15);
			_currentY += height + 15;
		}

		private void NewPolicyRow(string text, int height)
		{
			if (_currentY + height > 800)
			{
				AddNewPage();
			}
			WriteLine(text, _smallRegularFont, _initialX, 500, height);
			_currentY += height + 7;
		}

		private double GetTextHeight(string text, XFont font, double rectWidth)
		{
			double fontHeight = font.GetHeight();
			XSize measure = _gfx.MeasureString(text, font);
			double absoluteTextHeight = measure.Height;
			double absoluteTextWidth = measure.Width;

			if (absoluteTextWidth > rectWidth)
			{
				var linesToAdd = (int)Math.Ceiling(absoluteTextWidth / rectWidth) - 1;
				return absoluteTextHeight + linesToAdd * (fontHeight);
			}
			return absoluteTextHeight;
		}
	}
}
