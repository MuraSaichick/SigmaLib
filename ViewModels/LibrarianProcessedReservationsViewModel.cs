using ReactiveUI;
using SigmaLib.Models;
using SigmaLib.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace SigmaLib.ViewModels
{
    public class LibrarianProcessedReservationsViewModel : ViewModelBase
    {
        private User librarian;
        private ObservableCollection<Reservation> _reservations;
        private ReservationSearcher _reservationSearcher;
        private INavigationService _parent;
        private INavigationService _main;
        public User Librarian
        {
            get => librarian;
            set => this.RaiseAndSetIfChanged(ref librarian, value);
        }
        public ObservableCollection<Reservation> Reservations
        {
            get => _reservations;
            set => this.RaiseAndSetIfChanged(ref _reservations, value);
        }

        public ReactiveCommand<Unit, Unit> GoBackCommand { get; }
        public LibrarianProcessedReservationsViewModel(User user, INavigationService main, INavigationService parent)
        {
            Librarian = user;
            _parent = parent;
            _main = main;
            _reservationSearcher = new ReservationSearcher();
            Reservations = new ObservableCollection<Reservation>(_reservationSearcher.GetReservationByLibrarianId(Librarian.Id));
            GoBackCommand = ReactiveCommand.Create(() => { parent.NavigateTo(new ProfileViewModel(Librarian, main, _parent)); });
        }
    }
}
