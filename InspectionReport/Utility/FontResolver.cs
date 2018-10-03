using PdfSharp.Fonts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace InspectionReport.Utility
{
	public class FontResolver : IFontResolver
	{
		public byte[] GetFont(string faceName)
		{
			using (var ms = new MemoryStream())
			{
				using (var fs = File.Open(faceName, FileMode.Open))
				{
					fs.CopyTo(ms);
					ms.Position = 0;
					return ms.ToArray();
				}
			}
		}
		public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
		{
			if (familyName.Equals("Arial", StringComparison.CurrentCultureIgnoreCase))
			{
				if (isBold && isItalic)
				{
					return new FontResolverInfo("./Assets/arial.ttf");
				}
				else if (isBold)
				{
					return new FontResolverInfo("./Assets/arialbd.ttf");
				}
				else if (isItalic)
				{
					return new FontResolverInfo("./Assets/ariali.ttf");
				}
				else
				{
					return new FontResolverInfo("./Assets/arial.ttf");
				}
			}
			return null;
		}
	}
}
