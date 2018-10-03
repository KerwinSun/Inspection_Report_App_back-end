using PdfSharp.Fonts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace InspectionReport.Utility
{
	//public class FontResolver : IFontResolver
	//{
	//	public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
	//	{
	//		// Ignore case of font names.
	//		var name = familyName.ToLower().TrimEnd('#');

	//		// Deal with the fonts we know.
	//		switch (name)
	//		{
	//			case "arial":
	//				if (isBold)
	//				{
	//					if (isItalic)
	//					{
	//						return new FontResolverInfo("Arial#bi");
	//					}
	//					return new FontResolverInfo("Arial#b");
	//				}
	//				if (isItalic)
	//				{
	//					return new FontResolverInfo("Arial#i");
	//				}
	//				return new FontResolverInfo("Arial#");
	//		}

	//		// We pass all other font requests to the default handler.
	//		// When running on a web server without sufficient permission, you can return a default font at this stage.
	//		return PlatformFontResolver.ResolveTypeface(familyName, isBold, isItalic);
	//	}

	//	/// <summary>
	//	/// Return the font data for the fonts.
	//	/// </summary>
	//	public byte[] GetFont(string faceName)
	//	{
	//		switch (faceName)
	//		{
	//			case "Arial#":
	//				return FontHelper.Arial;

	//			case "Arial#b":
	//				return FontHelper.ArialBold;

	//			case "Arial#i":
	//				return FontHelper.ArialItalic;
	//		}

	//		return null;
	//	}


	//	internal static FontResolver OurGlobalFontResolver = null;

	//	/// <summary>
	//	/// Ensure the font resolver is only applied once (or an exception is thrown)
	//	/// </summary>
	//	internal static void Apply()
	//	{
	//		if (OurGlobalFontResolver == null || GlobalFontSettings.FontResolver == null)
	//		{
	//			if (OurGlobalFontResolver == null)
	//				OurGlobalFontResolver = new FontResolver();

	//			GlobalFontSettings.FontResolver = OurGlobalFontResolver;
	//		}
	//	}
	//}


	///// <summary>
	///// Helper class that reads font data from embedded resources.
	///// </summary>
	//public static class FontHelper
	//{
	//	public static byte[] Arial
	//	{
	//		get { return LoadFontData("Assets.arial.ttf"); }
	//	}

	//	public static byte[] ArialBold
	//	{
	//		get { return LoadFontData("Assets.arialbd.ttf"); }
	//	}

	//	public static byte[] ArialItalic
	//	{
	//		get { return LoadFontData("Assets.ariali.ttf"); }
	//	}

	//	/// <summary>
	//	/// Returns the specified font from an embedded resource.
	//	/// </summary>
	//	static byte[] LoadFontData(string name)
	//	{
	//		var assembly = Assembly.GetExecutingAssembly();

	//		// Test code to find the names of embedded fonts
	//		//var ourResources = assembly.GetManifestResourceNames();

	//		using (Stream stream = assembly.GetManifestResourceStream(name))
	//		{
	//			if (stream == null)
	//				throw new ArgumentException("No resource with name " + name);

	//			int count = (int)stream.Length;
	//			byte[] data = new byte[count];
	//			stream.Read(data, 0, count);
	//			return data;
	//		}
	//	}
	//}
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
