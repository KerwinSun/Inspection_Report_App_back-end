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
            sb.AppendFormat(
               @"
<div class='topHeaderStyling'>
    <h1>
Hitch Building Inspections
</h1>
</div>
<table border='0'>
    <tr>
        <td style='padding-left: 10px; padding-bottom: 1em;'>
            <b>
            Date of Inspection:
</b>
        </td>
        <td style='padding-left: 10px; padding-bottom: 1em;'>
            {0}
        </td>
    </tr>
    <tr>
        <td style='padding-left: 10px; padding-bottom: 1em;'>
            <b>
            Client Information:
</b>
        </td>
        <td style='padding-left: 10px; padding-bottom: 1em;'>
            {1}
        </td>
    </tr>
    <tr>
        <td style='padding-left: 10px; padding-bottom: 1em;'>
            <b>
            Address Inspected:
</b>
        </td>
        <td style='padding-left: 10px; padding-bottom: 1em;'>
            {2}
        </td>
    </tr>
    <tr>
        <td style='padding-left: 10px; padding-bottom: 1em;'>
            <b>
            Summonsed By:
</b>
        </td>
        <td style='padding-left: 10px; padding-bottom: 1em;'>
            {3}
        </td>
    </tr>
    <tr>
        <td style='padding-left: 10px; padding-bottom: 1em;'>
            <b>
            Inspected By:
</b>
        </td>
        <td style='padding-left: 10px; padding-bottom: 1em;'>
            {4}
        </td>
    </tr>
    <tr>
        <td style='padding-left: 10px; padding-bottom: 1em;'>
            <b>
            Contact Details:
</b>
        </td>
        <td style='padding-left: 10px; padding-bottom: 1em;'>
            {5}
        </td>
    </tr>
    <tr>
        <td style='padding-left: 10px; padding-bottom: 1em;'>
            <b>
            Home Number:
</b>
        </td>
        <td style='padding-left: 10px; padding-bottom: 1em;'>
            {6}
        </td>
    </tr>
    <tr>
        <td style='padding-left: 10px; padding-bottom: 1em;'>
            <b>
            Mobile Number:
</b>
        </td>
        <td style='padding-left: 10px; padding-bottom: 1em;'>
            {7}
        </td>
    </tr>
    <tr>
        <td style='padding-left: 10px; padding-bottom: 1em;'>
            <b>
            Address:
</b>
        </td>
        <td style='padding-left: 10px; padding-bottom: 1em;'>
            {8}
        </td>
    </tr>
    <tr>
        <td style='padding-left: 10px; padding-bottom: 1em;'>
            <b>
            Email Address:
</b>
        </td>
        <td style='padding-left: 10px; padding-bottom: 1em;'>
            {9}
        </td>
    </tr>
    <tr>
        <td style='padding-left: 10px; padding-bottom: 1em;'>
            <b>
            Real Estate & Agent:
</b>
        </td>
        <td style='padding-left: 10px; padding-bottom: 1em;'>
            {10}
        </td>
    </tr>
    <tr>
        <td style='padding-left: 10px; padding-bottom: 1em;'>
            <b>
            House Description:
</b>
        </td>
        <td style='padding-left: 10px; padding-bottom: 1em;'>
            {11}
        </td>
    </tr>
    <tr>
        <td style='padding-left: 10px; padding-bottom: 1em;'>
            <b>
            Estimate Summary:
</b>
        </td>
        <td style='padding-left: 10px; padding-bottom: 1em;'>
            {12}
        </td>
    </tr>
    <tr>
        <td style='padding-left: 10px; padding-bottom: 1em;'>
            <b>
            Rooms Summary:
</b>
        </td>
        <td style='padding-left: 10px; padding-bottom: 1em;'>
            {13}
        </td>
    </tr>
    <tr>
        <td style='padding-left: 10px; padding-bottom: 1em;'>
            <b>
            Construction Types:
</b>
        </td>
        <td style='padding-left: 10px; padding-bottom: 1em;'>
            {14}
        </td>
    </tr>
</table>",
                house.InspectionDate.ToShortDateString(),
                "Robert Kirkpatrick",
                house.Address,
                "Frano Stanisic",
                inspectors,
                "r.kirkpatrick@auckland.ac.nz",
                "home number",
                "phone number",
                "address",
                "email address",
                "real estate & agent",
                "Rob's House",
                "estimate summary",
                "rooms summary",
                house.ConstructionType
            );
        }

        private void GenerateHousePage(StringBuilder sb, House house)
        {
            foreach (Category category in house.Categories)
            {
                sb.Append(@"<br />");
                sb.AppendFormat(@"
					<table border='1'>
					<tr>
						<th style='padding-left: 10px;'>
Category Name
</th>
						<th style='padding-left: 10px;'>
Count
</th>
					</tr>
					<tr>
						<td style='padding-left: 10px;'>
<h3>
{0}
</h3>
</td>
						<td style='padding-left: 10px;'>{1}</td>
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
					<table border='1'>
					
					<tr>
						<th class='nameStyling' style='padding-left: 10px;'>
Name
</th>
						<th class='gradeStyling' style='padding-left: 10px;'>
Grade
</th>
						<th class='commentStyling' style='padding-left: 10px;'>
Comment
</th>
					</tr>
					");

            foreach (Feature feature in category.Features)
            {
                string name = feature.Name;
                string featureGrade = translateFeatureGrade(feature.Grade);
                string comment = feature.Comments;

                sb.AppendFormat(@"
						<tr>
							<td style='padding-left: 10px;'>
<b>
{0}
</b>
</td>
							<td style='padding-left: 10px;'>
{1}
</td>
							<td style='padding-left: 10px;'>
{2}
</td>
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
//                return "Good";
                return "A";
            if (gradeNumber == 2)
                //                return "Will need attention soon";
                return "B";
            if (gradeNumber == 3)
                //                return "Will need immediate attention";
                return "C";
            if (gradeNumber == 4)
                //                return "N/A";
                return "D";

            return null;
        }
    }
}