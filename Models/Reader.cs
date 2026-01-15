using SigmaLib.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SigmaLib.Models
{
    public class Reader : User
    {
        
        private readonly CatalogService _catalogService;
        private readonly ReservationSearcher _reservationSearcher;
        public Reader()
        {
            _catalogService = new CatalogService();
            _reservationSearcher = new ReservationSearcher();
        }
        public Reader(User user)
        {
            _catalogService = new CatalogService();
            _reservationSearcher = new ReservationSearcher();
            Id = user.Id;
            Email = user.Email;
            FirstName = user.FirstName;
            LastName = user.LastName;
            SurName = user.SurName;
            Phone = user.Phone;
            Address = user.Address;
            Role = user.Role;
            IsBlocked = user.IsBlocked;
            Features = new ReaderFeatures();
            Dept = user.Dept;
        }
        public List<Book> GetAllBooks() => _catalogService.GetAllBooks();
        public List<Book> GetBooksByParametrs(string title = null, string author = null, int? year = null, string genre = null) => _catalogService.SearchBooks(author, title, year, genre);
    }
}