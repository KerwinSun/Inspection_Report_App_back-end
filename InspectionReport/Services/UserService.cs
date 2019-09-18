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
            if (!user.AccountType.HasValue){
                user.AccountType = UserAccountType.Client;
            }
            
            return user;
        }

        public List<UserDTO> DTOConvert(List<User> users)
        {
            List<UserDTO> userDTOList = new List<UserDTO>();
            foreach (User user in users){
                var userDTO = new UserDTO();
                userDTO.UpdateObjectFromOther(user);

                userDTOList.Add(userDTO);
            }
            return userDTOList;
        }

        //public UserDTO UserToUserDTO(User user)
        //{
        //    UserDTO userDTO{
        //        var 
        //    }
        //}
        
    }
}
