using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SigmaLib.Interfaces;

namespace SigmaLib.Models
{
    public class AdminFeatures : IProfileFeatures
    {
        public bool CanViewCreatedReservations { get; } = false;
        public bool CanViewProcessedReservations { get; } = false;
        public bool CanManageThisAccaunt { get; } = true;
    }
    public class LibrarianFeatures : IProfileFeatures
    {
        public bool CanViewCreatedReservations { get; } = false;
        public bool CanViewProcessedReservations { get; } = true;
        public bool CanManageThisAccaunt { get; } = true;
    }
    public class ReaderFeatures : IProfileFeatures
    {
        public bool CanViewCreatedReservations { get; } = true;
        public bool CanViewProcessedReservations { get; } = false;
        public bool CanManageThisAccaunt { get; } = true;
    }
    public class CheckReaderFeatures : IProfileFeatures
    {
        public bool CanViewCreatedReservations { get; } = true;
        public bool CanViewProcessedReservations { get; } = false;
        public bool CanManageThisAccaunt { get; } = false;
    }
}
