using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SigmaLib.Interfaces
{
    public interface IProfileFeatures
    {
        bool CanViewCreatedReservations { get; }   // читатель
        bool CanViewProcessedReservations { get; } // библиотекарь
        bool CanManageThisAccaunt { get; } // просмотр данных
    }
}
