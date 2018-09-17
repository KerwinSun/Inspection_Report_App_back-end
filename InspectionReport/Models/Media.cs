using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InspectionReport.Models
{
    public class Media
    {
        public long Id { get; set; }
        public Feature Feature { get; set; }
        public string MediaName { get; set; } 
    }
}
