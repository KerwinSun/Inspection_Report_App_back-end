using InspectionReport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InspectionReport.Services
{
    public class UserService
    {
        public UserService() { }

        public User UserCheck(User user)
        {
            if (!user.Type.HasValue){
                user.Type = AccountType.Client;
            }
            
            return user;
        }
        
    }
}
