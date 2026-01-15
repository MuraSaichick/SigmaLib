using ReactiveUI;
using SigmaLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SigmaLib.Services;
using SigmaLib.Models;

namespace SigmaLib.ViewModels
{
    public class LibraryStatsViewModel : ViewModelBase
    {
        private readonly Admin _admin;
        private readonly LibraryStatsService _libraryStatsService;
        public LibraryStatsViewModel(Admin admin)
        {
            _admin = admin;
            _libraryStatsService = new LibraryStatsService();
            RefreshStats();
        }
        private int _activeUsersCount;
        public int ActiveUsersCount
        {
            get => _activeUsersCount;
            set => this.RaiseAndSetIfChanged(ref _activeUsersCount, value);
        }

        // Количество бронирований
        private int _reservationsCount;
        public int ReservationsCount
        {
            get => _reservationsCount;
            set => this.RaiseAndSetIfChanged(ref _reservationsCount, value);
        }

        // Количество книг в каталоге
        private int _booksCount;
        public int BooksCount
        {
            get => _booksCount;
            set => this.RaiseAndSetIfChanged(ref _booksCount, value);
        }

        // Сумма неоплаченных штрафов
        private decimal _unpaidFinesSum;
        public decimal UnpaidFinesSum
        {
            get => _unpaidFinesSum;
            set => this.RaiseAndSetIfChanged(ref _unpaidFinesSum, value);
        }
        private void RefreshStats()
        {
            ActiveUsersCount = _libraryStatsService.LoadActiveUsersCount();
            BooksCount = _libraryStatsService.LoadBooksCount();
            ReservationsCount = _libraryStatsService.LoadReservationsCount();
            UnpaidFinesSum = _libraryStatsService.LoadTotalFinesSum();
        }

    }
}
