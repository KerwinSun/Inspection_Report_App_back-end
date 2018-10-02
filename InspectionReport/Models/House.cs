using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using InspectionReport.Models.Converter;

namespace InspectionReport.Models
{
    /// <summary>
    /// A House entity represents a inspection report for a particular
    /// house.
    /// </summary>
    public class House : IUpdatable<House>
    {
        public House ()
        {
            InspectedBy = new List<HouseUser>();
            Categories = new List<Category>();
        }

        public long Id { get; set; }
        public Boolean Completed { get; set; }
        public string Address { get; set; }
        public ICollection<HouseUser> InspectedBy { get; set; }
        public Client SummonsedBy { get; set; }
        public string ConstructionType { get; set; }
        public string Comments { get; set; }
        public string PrivateComments { get; set; }
        public string EstimateSummary { get; set; }
        public string RoomsSummary { get; set; }
        public AreaInspected AreaInspected { get; set; }


        [JsonProperty]
        [JsonConverter(typeof(ESDateTimeConverter))]
        public DateTime InspectionDate { get; set; }
        [InverseProperty("House")]
        public ICollection<Category> Categories { get; set; }

        public void UpdateObjectFromOther (House other)
        {
            Completed = other.Completed;
            Address = other.Address;
            ConstructionType = other.ConstructionType;
            InspectionDate = other.InspectionDate;
            Comments = other.Comments;
            EstimateSummary = other.EstimateSummary;
            RoomsSummary = other.RoomsSummary;
            PrivateComments = other.PrivateComments;


            //add all new HouseUser objects
            foreach (HouseUser hu in other.InspectedBy ?? Enumerable.Empty<HouseUser>())
            {
                //attempt to find the house user in this
                bool existInCurrent = InspectedBy
                    .Where(thisHu => hu.UserId == thisHu.UserId)
                    .Count() > 0;
                if (!existInCurrent)
                {
                    InspectedBy.Add(hu);
                }
            }

            //Update all "existing" categories
            foreach (Category category in Categories)
            {
                Category otherCategory = other.Categories.Where(c => c.Id == category.Id).SingleOrDefault();
                if (otherCategory == null)
                {
                    //Perhaps the other category is deleted, so no longer in new house?
                    //Do nothing for now, as deleting categories are not expected.
                }
                else
                {
                    category.UpdateObjectFromOther(otherCategory);
                }
            }

            //Create all new categories.
            ICollection<Category> newCategories = other.Categories.Where(c => c.Id == 0).ToList();
            foreach (Category newCategory in newCategories)
            {
                Categories.Add(newCategory);
            }
        }
    }
}
