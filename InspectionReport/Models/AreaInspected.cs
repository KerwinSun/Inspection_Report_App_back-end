using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InspectionReport.Models
{
    /// <summary>
    /// An area inspected is the area which an inspector inspected during their inspection.
    /// </summary>
    public class AreaInspected
    {

        public long Id { get; set; }
        public bool Site { get; set; }
        public bool Subfloor { get; set; }
        public bool Exterior { get; set; }
        public bool RoofExterior { get; set; }
        public bool RoofSpace { get; set; }
        public bool Services { get; set; }
        public bool Other { get; set; }

    }
}
