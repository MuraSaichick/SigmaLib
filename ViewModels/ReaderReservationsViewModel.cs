using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using SigmaLib.Models;
using SigmaLib.Services;

namespace SigmaLib.ViewModels
{
    public class ReaderReservationsViewModel : ViewModelBase
    {
        private User reader;
        private ObservableCollection<Reservation> _reservations;
        private ReservationSearcher _reservationSearcher;
        private INavigationService _parent;
        private INavigationService _main;
        public User Reader
        {
            get => reader;
            set => this.RaiseAndSetIfChanged(ref reader, value);
        }
        public ObservableCollection<Reservation> Reservations
        {
            get => _reservations;
            set => this.RaiseAndSetIfChanged(ref _reservations, value);
        }

        public ReactiveCommand<Unit, Unit> GoBackCommand { get; }
        public ReaderReservationsViewModel(User user, INavigationService main, INavigationService parent)
        {
            Reader = user;
            _parent = parent;
            _main = main;
            _reservationSearcher = new ReservationSearcher();
            Reservations = new ObservableCollection<Reservation>(_reservationSearcher.GetReservationByReaderId(reader.Id));
            GoBackCommand = ReactiveCommand.Create(() => { parent.NavigateTo(new ProfileViewModel(Reader, main, _parent)); });
        }
    }
}