using SigmaLib.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SigmaLib.Models
{
    public class Librarian : User
    {
        private readonly UserSearcher _userSearcher;
        public Librarian()
        {
            _userSearcher = new UserSearcher();
        }
        public Librarian(User user)
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
            Features = new LibrarianFeatures();
            _userSearcher = new UserSearcher();
        }
    }
}
