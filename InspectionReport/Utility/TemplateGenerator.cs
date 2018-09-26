using System;
using System.Collections.Generic;
using System.Text;
using InspectionReport.Models;
using System.Linq;
using InspectionReport.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace InspectionReport.Utility
{
    public class TemplateGenerator
    {
        private ReportContext _context;

        private ImageController _iController;

        private Boolean generateImageSection = false;

        public TemplateGenerator(ReportContext context)
        {
            _context = context;
            _iController = new ImageController(_context);
        }

        public string Generate(House house, string inspectors)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(
                @"
                <html>
                    <head>
                    </head>
                    <body>"
            );

            GenerateTitlePage(sb, house, inspectors);
            GenerateHousePage(sb, house);

            if (generateImageSection)
            {
                GenerateImageSection(sb, house);
            }

            sb.Append(
                @"
					</body>
				</html>"
            );

            return sb.ToString();
        }


        private void GenerateImageSection(StringBuilder sb, House house)
        {
            sb.Append(
                @"
				<div>
					<h1>
						Images
					</h1>
				</div>
				"
            );

            foreach (Category category in house.Categories)
            {
                int sum = 0;

                foreach (Feature feature in category.Features)
                {
                    int count = _context.Media
                        .Where(m => m.Feature == feature).ToList().Count;

                    sum = sum + count;
                }

                if (sum > 0)
                {
                    sb.AppendFormat(@"
				<div>
					<h2>
						{0}
					</h2>
				</div>
			        ", category.Name);
                }

                foreach (Feature feature in category.Features)
                {
                    List<Media> medias = _context.Media
                        .Where(m => m.Feature == feature).ToList();

                    if (medias != null && medias.Count != 0)
                    {
                        sb.AppendFormat(@"
				<div>
					<h3>
						{0}
					</h3>
				</div>
			        ", feature.Name);
                    }

                    OkObjectResult mediaQueryResult = _iController.GetImage(feature.Id).Result as OkObjectResult;
                    List<string> URIResults = mediaQueryResult.Value as List<string>;

                    foreach (string URIResult in URIResults)
                    {
                        sb.AppendFormat(@"
				<div>
					<img src='{0}' alt='Feature Image'>
				</div>
			        ",
                            URIResult.ToString()
                        );

                        sb.Append(@"<br />");
                    }
                }
            }
        }

        private void GenerateTitlePage(StringBuilder sb, House house, string inspectors)
        {
            sb.AppendFormat(@"
				<div class='topHeaderStyling'>
<h1>
Hitch Building Inspections
</h1>
</div>
				<table>
					<tr>
						<td>Date of Inspection:</td>
						<td>{0}</td>
					</tr>
					<tr>
						<td>Client Information:</td>
					</tr>
					<tr>
						<td>Address Inspected:</td>
						<td>{1}</td>
					</tr>
					<tr>
						<td>Summonsed By:</td>
						<td>{2}</td>
					</tr>
					<tr>
						<td>Inspected By:</td>
						<td>{3}</td>
					</tr>
					<tr>
						<td>Contact Details:</td>
					</tr>
					<tr>
						<td>Home Number:</td>
						<td>{4}</td>
					</tr>
					<tr>
						<td>Mobile Number:</td>
						<td>{5}</td>
					</tr>
					<tr>
						<td>Address:</td>
						<td>{6}</td>
					</tr>
					<tr>
						<td>Email Address:</td>
						<td>{6}</td>
					</tr>
					<tr>
						<td>Real Estate & Agent:</td>
						<td>{8}</td>
					</tr>
					<tr>
						<td>House Description:</td>
					</tr>
					<tr>
						<td>Estimate Summary:</td>
						<td>{9}</td>
					</tr>
					<tr>
						<td>Rooms Summary:</td>
						<td>{10}</td>
					</tr>
					<tr>
						<td>Construction Types:</td>
						<td>{11}</td>
					</tr>
				</table>
				", house.InspectionDate, house.Address, "Frano Stanisic", inspectors, "home number",
                "phone number", "address", "email address", "real estate & agent", "estimate summary", "rooms summary",
                house.ConstructionType
            );
        }

        private void GenerateHousePage(StringBuilder sb, House house)
        {
            foreach (Category category in house.Categories)
            {
                sb.Append(@"<br />");
                sb.AppendFormat(@"
					<table>
					<tr>
						<th>Category Name</th>
						<th>Count</th>
					</tr>
					<tr>
						<td>
<h3>
{0}
</h3>
</td>
						<td>{1}</td>
					</tr>
			        ", category.Name, category.Count);

                sb.Append(@"</table>");
                sb.Append(@"<br />");

                appendFeatureTable(sb, category);
            }
        }

        private StringBuilder appendFeatureTable(StringBuilder sb, Category category)
        {
            sb.Append(@"
					<table>
					
					<tr>
						<th class='nameStyling'>Name</th>
						<th class='gradeStyling'>Grade</th>
						<th class='commentStyling'>Comment</th>
					</tr>
					");

            foreach (Feature feature in category.Features)
            {
                string name = feature.Name;
                string featureGrade = translateFeatureGrade(feature.Grade);
                string comment = feature.Comments;

                sb.AppendFormat(@"
						<tr>
							<td>
<b>
{0}
</b>
</td>
							<td>{1}</td>
							<td>{2}</td>
						</tr>",
                    name,
                    featureGrade,
                    comment
                );
            }

            sb.Append(@"</table>");
            sb.Append(@"<br />");
            return sb;
        }

        private string translateFeatureGrade(int? gradeNumber)
        {
            if (gradeNumber == null)
                return null;
            if (gradeNumber == 1)
                return "Good";
            if (gradeNumber == 2)
                return "Will need attention soon";
            if (gradeNumber == 3)
                return "Will need immediate attention";
            if (gradeNumber == 4)
                return "N/A";

            return null;
        }
    }
}