using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InspectionReport.Models
{
    public class Category
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public House House { get; set; }
    }
}
