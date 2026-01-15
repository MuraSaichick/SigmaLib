using SigmaLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SigmaLib.Interfaces
{
    public interface IReservationCreationService
    {
        public bool CreateReservation(int bookId, int userId, DateTime pickupDate, DateTime returnDate, decimal fine);
        //public bool CreateReservation(Reservation reservation);
    }
}
