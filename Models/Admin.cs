using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using SigmaLib.Services;
using System.Collections.ObjectModel;

namespace SigmaLib.Models
{
    public class Admin : User
    {
        private readonly UserService _userService;
        private readonly UserSearcher _userSearcher;

        public Admin(UserService userService, UserSearcher userSearcher)
        {
            _userService = userService;
            _userSearcher = userSearcher;
            Role = UserRole.Admin;
        }
        public Admin()
        {
            _userService = new UserService();
            _userSearcher = new UserSearcher();
            Role = UserRole.Admin;
        }
        public Admin(User user)
        {
            Id = user.Id;
            Email = user.Email;
            FirstName = user.FirstName;
            LastName = user.LastName;
            SurName = user.SurName;
            Phone = user.Phone;
            Address = user.Address;
            Role = user.Role;
            IsBlocked = user.IsBlocked;
            Features = new AdminFeatures();
            _userService = new UserService();
            _userSearcher = new UserSearcher();
        }

        // Методы-обёртки для удобства
        public OperationResult BlockUser(int id) => _userService.BlockUser(id);
        public OperationResult UnblockUser(int id) => _userService.UnblockUser(id);
        public OperationResult DeleteUser(int id) => _userService.DeleteUser(id);
        public OperationResult CreateUser(User user) => _userService.CreateUser(user);

      
        public List<User> GetAllUsers() => _userSearcher.GetAllUsers();
    }
}