using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SigmaLib.Models;
using SigmaLib.Services;
using ReactiveUI;
using System.Reactive;

namespace SigmaLib.ViewModels
{
    public class LibrarianMainMenuViewModel : ViewModelBase, INavigationService
    {
        private Librarian _librarian;
        private INavigationService _main;
       
        private ViewModelBase _currentViewModel;
        private BookManagementViewModel _bookManagmentViewModel;
        private ProfileViewModel _profileViewModel;
        private ReservationManagementViewModel _reservationManagementViewModel;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
        }
        public Librarian CurrentLibrarian
        {
            get => _librarian;
            set => this.RaiseAndSetIfChanged(ref _librarian, value);
        }

        public ReactiveCommand<Unit, Unit> GoToProfile { get; }
        public ReactiveCommand<Unit, Unit> GoToBookManagement { get; }
        public ReactiveCommand<Unit, Unit> GoToReservationManagementCommand { get; }
        public ReactiveCommand<Unit, Unit> GoToReaderManagementCommand { get; }

        public LibrarianMainMenuViewModel(Librarian librarian, INavigationService main)
        {
            this._librarian = librarian;
            _main = main;

            CurrentViewModel = new BookManagementViewModel(this, _librarian, this); ;
            GoToProfile = ReactiveCommand.Create(() => { CurrentViewModel = new ProfileViewModel(_librarian, _main, this); });
            GoToBookManagement = ReactiveCommand.Create(() => { CurrentViewModel = new BookManagementViewModel(this, _librarian, this); });
            GoToReservationManagementCommand = ReactiveCommand.Create(() => { CurrentViewModel = new ReservationManagementViewModel(_librarian); });
            GoToReaderManagementCommand = ReactiveCommand.Create(() => { CurrentViewModel = new ReaderManagementViewModel(_librarian); });
 
        }
        public void NavigateTo(ViewModelBase viewModel)
        {
            CurrentViewModel = viewModel;
        }
    }
}
