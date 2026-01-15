using ReactiveUI;
using SigmaLib.Interfaces;
using SigmaLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SigmaLib.ViewModels;
using System.Reactive;
using SigmaLib.Services;

namespace SigmaLib.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {
        private User _user;
        private INavigationService _main;
        private INavigationService _parent;
        public User CurrentUser
        {
            get => _user;
            set => this.RaiseAndSetIfChanged(ref _user, value);
        }

        public ReactiveCommand<Unit, Unit> Logout { get; }
        public ReactiveCommand<Unit, Unit> GoToCheckReservationsCommand { get; }
        public ReactiveCommand<Unit, Unit> GoToCheckProcessedReservationsCommand { get; }
        public ProfileViewModel(User user, INavigationService main, INavigationService parent)
        {
            CurrentUser = user;
            _main = main;
            Logout = ReactiveCommand.Create(() => { _main.NavigateTo(new LoginViewModel(_main)); });
            GoToCheckReservationsCommand = ReactiveCommand.Create(() => { _parent.NavigateTo(new ReaderReservationsViewModel(CurrentUser, _main, parent)); });
            GoToCheckProcessedReservationsCommand = ReactiveCommand.Create(() => { _parent.NavigateTo(new LibrarianProcessedReservationsViewModel(CurrentUser, _main, parent)); });
            _parent = parent;
        }
    }
}