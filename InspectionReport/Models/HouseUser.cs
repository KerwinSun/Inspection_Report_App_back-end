using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InspectionReport.Models
{
    public class HouseUser
    {
        public long UserId { get; set; }
        public User User { get; set; }
        public long HouseId { get; set; }
        public House House { get; set; }
    }
}
