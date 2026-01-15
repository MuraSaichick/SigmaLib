using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SigmaLib.Models;

namespace SigmaLib.Interfaces
{
    public interface IReservationQueryService
    {
        List<Reservation> GetAllReservation();
        List<Reservation> GetReservationByFilter(Reservation reservationTemplate);
        List<Reservation> GetReservationByReaderId(int ReaderId);
        string GetTitleById(int id);
    }
}
