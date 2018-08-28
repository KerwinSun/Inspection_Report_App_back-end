using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InspectionReport.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public ICollection<HouseUser> Inspected { get; set; }
    }
}