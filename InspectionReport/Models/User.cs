using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InspectionReport.Models
{
    /// <summary>
    /// A user is a inspector/ admin who can access inspection reports.
    /// </summary>
    public class User
    {
        public User ()
        {
            Inspected = new List<HouseUser>();
        }

        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public ICollection<HouseUser> Inspected { get; set; }
        public ApplicationUser AppLoginUser { get; set; }
        public void UpdateObjectFromOther(User other)
        {
            FirstName = other.FirstName;
            LastName = other.LastName;
            Email = other.Email;
            Phone = other.Email;
            Password = other.Email;
        }
    }


}