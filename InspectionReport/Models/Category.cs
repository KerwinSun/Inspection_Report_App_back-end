using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InspectionReport.Models
{
    /// <summary>
    /// The Cateogries represent the sections within a house
    /// E.g. Kitchen, Rooms, Garden etc. I.e. Anything inside
    /// or outside the house. 
    /// </summary>
    public class Category : IUpdatable<Category>
    {
        public Category ()
        {
            Features = new List<Feature>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        [Required]
        public House House { get; set; }

        public ICollection<Feature> Features { get; set; }
        public void UpdateObjectFromOther(Category other)
        {
            Name = other.Name;
            Count = other.Count;

            //Update all "existing" features
            foreach (Feature feature in Features)
            {
                Feature otherFeature = other.Features.Where(f => f.Id == feature.Id).SingleOrDefault();
                if (otherFeature == null)
                {
                    //Perhaps the other category is deleted, so no longer in new house?
                    //Do nothing for now, as deleting categories are not expected.
                }
                else
                {
                    feature.UpdateObjectFromOther(otherFeature);
                }
            }

            //Create all new features.
            ICollection<Feature> newFeatures = other.Features.Where(f => f.Id == 0).ToList();
            foreach (Feature newFeature in newFeatures)
            {
                Features.Add(newFeature);
            }
        }
    }
}
