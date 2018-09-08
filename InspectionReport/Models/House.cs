﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations.Schema;

namespace InspectionReport.Models
{
    /// <summary>
    /// A House entity represents a inspection report for a particular
    /// house.
    /// </summary>
    public class House
    {
        public long Id { get; set; }
        public string Address { get; set; }
        public ICollection<HouseUser> InspectedBy { get; set; }
        public string ConstructionType { get; set; }
        public DateTime InspectionDate { get; set; }
        [InverseProperty("House")]
        public ICollection<Category> Categories { get; set; }
    }
}
