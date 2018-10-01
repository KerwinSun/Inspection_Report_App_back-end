using PdfSharpCore.Fonts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace InspectionReport.Utility
{
	public class FontResolver : IFontResolver
	{
		public string DefaultFontName => throw new NotImplementedException();

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
			if (familyName.Equals("OpenSans", StringComparison.CurrentCultureIgnoreCase))
			{
				if (isBold && isItalic)
				{
					return new FontResolverInfo(@".\Assets\OpenSans-BoldItalic.ttf");
				}
				else if (isBold)
				{
					return new FontResolverInfo(@".\Assets\OpenSans-Bold.ttf");
				}
				else if (isItalic)
				{
					return new FontResolverInfo(@".\Assets\OpenSans-Italic.ttf");
				}
				else
				{
					return new FontResolverInfo(@".\Assets\OpenSans-Regular.ttf");
				}
			}
			return null;
		}
	}
}
