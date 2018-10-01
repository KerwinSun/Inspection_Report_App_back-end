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

		public ExportController(ReportContext context)
		{
			_context = context;
		}

		public ActionResult GetPDF()
		{
			GlobalFontSettings.FontResolver = new FontResolver();

			PdfDocument document = new PdfDocument();
			PdfPage page = document.AddPage();
			XGraphics gfx = XGraphics.FromPdfPage(page);
			XFont font = new XFont("OpenSans", 20, XFontStyle.Bold);
			gfx.DrawString("This is my first PDF document", font, XBrushes.Black, new XRect(0, 0, page.Width, page.Height), XStringFormats.Center);
			string pdfFilename = "firstpage.pdf";

			using (MemoryStream ms = new MemoryStream())
			{
				document.Save(ms, false);
				byte[] buffer = new byte[ms.Length];
				ms.Seek(0, SeekOrigin.Begin);
				ms.Flush();
				ms.Read(buffer, 0, (int)ms.Length);
				byte[] docBytes = ms.ToArray();
				return File(docBytes, "application/pdf", pdfFilename);
			}
		}
		//[HttpGet("{id}", Name = "GetPDF")]
		//public IActionResult GetPDF(long id)
		//{
		//	byte[] file = GeneratePDF(id);

		//	return file == null ? (IActionResult) NotFound() : File(file, "application/pdf");
		//}

		//private byte[] GeneratePDF(long id)
		//{
		//	House house = _context.House
		//		.Where(h => h.Id == id)
		//		.Include(h => h.Categories)
		//		.ThenInclude(c => c.Features)
		//		.Include(h => h.InspectedBy)
		//		.SingleOrDefault();

		//	if (house == null)
		//	{
		//		return null;
		//	}

		//	string names = "";

		//	foreach (var hu in house.InspectedBy)
		//	{
		//		User user = _context.Users
		//			.Where(u => u.Id == hu.UserId)
		//			.SingleOrDefault();

		//		if (names == "")
		//		{
		//			names = names + user.Name;
		//		}
		//		else
		//		{
		//			names = names + ", " + user.Name;
		//		}
		//	}

		//	return null;
		//}
	}
}
