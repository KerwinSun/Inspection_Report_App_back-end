using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InspectionReport.Models.Converter
{
    public class ESDateTimeConverter : IsoDateTimeConverter
    {
        public ESDateTimeConverter()
        {
            base.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss";
        }
    }
}
