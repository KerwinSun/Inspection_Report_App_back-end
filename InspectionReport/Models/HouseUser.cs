using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InspectionReport.Models
{
    /// <summary>
    /// The HouseUser Table represents the Many-To-Many relationship
    /// between the House and User Tables. A House can be associated with 
    /// many users (inspectors) and one user (inspector) will be inspecting
    /// many houses.
    /// </summary>
    public class HouseUser
    {
        public long UserId { get; set; }
        [Required]
        public User User { get; set; }
        public long HouseId { get; set; }
        [Required]
        public House House { get; set; }
    }
}
