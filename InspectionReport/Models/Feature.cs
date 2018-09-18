﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InspectionReport.Models
{
    /// <summary>
    /// Features are object/ items found within a category.
    /// E.g. Within a Kitchen category is a benchtop, bar-stools etc.
    /// </summary>
    public class Feature : IUpdatable<Feature>
    {
        public long Id { get; set; }
        [Range(1,5)]
        public int? Grade { get; set; }
        public string Name { get; set; }
        public string Comments { get; set; }
        public Category Category { get; set; }
        public ICollection<Media> ImageFileNames { get; set; }

        public void UpdateObjectFromOther (Feature other)
        {
            Grade = other.Grade;
            Name = other.Name;
            Comments = other.Comments;
        }
    }
}