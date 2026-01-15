using SigmaLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SigmaLib.Interfaces
{
    public interface IReservationManagementService : IReservationCreationService
    {
        void AcceptReservation( Reservation reservation, int librarianId);
        void RejectReservation( Reservation reservation, int librarianId);
        OperationResult ProcessReturnBook(Reservation reservation, User reader, User librarian, Book book);
    }
}
