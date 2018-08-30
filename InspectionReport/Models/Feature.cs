using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InspectionReport.Models
{
    public class Feature
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }
        public Category Category { get; set; }

        //public Media Media { get; set; }
    }
}
