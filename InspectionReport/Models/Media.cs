using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InspectionReport.Models
{
    public class Media
    {
        public long Id { get; set; }
        [Required]
        public Feature Feature { get; set; }
        [Required]
        public string MediaName { get; set; } 
    }
}
