using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InspectionReport.Models
{
    /// <summary>
    /// The Cateogries represent the sections within a house
    /// E.g. Kitchen, Rooms, Garden etc. I.e. Anything inside
    /// or outside the house. 
    /// </summary>
    public class Category
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public House House { get; set; }

        public ICollection<Feature> Features { get; set; }

    }
}
