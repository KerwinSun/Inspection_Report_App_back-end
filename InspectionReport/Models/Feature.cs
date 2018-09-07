using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InspectionReport.Models
{
    /// <summary>
    /// Features are object/ items found within a category.
    /// E.g. Within a Kitchen category is a benchtop, bar-stools etc.
    /// </summary>
    public class Feature
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }
        public Category Category { get; set; }
    }
}
