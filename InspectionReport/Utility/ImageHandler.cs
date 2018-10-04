
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using PdfSharp.Drawing;

namespace InspectionReport.Utility
{
	public class ImageHandler
	{
		public XImage FromURI(string uri)
		{
			HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(uri);
			webRequest.AllowWriteStreamBuffering = true;
			WebResponse webResponse = webRequest.GetResponse();
			XImage xImage = XImage.FromStream(webResponse.GetResponseStream());
			webResponse.Close();
			return xImage;
		}
	}
}
