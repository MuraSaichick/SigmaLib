using ReactiveUI;
using SigmaLib.Models;
using SigmaLib.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SigmaLib.ViewModels
{
    public class AdminMainMenuViewModel : ViewModelBase
    {
        private INavigationService _main;
        public UserManagementViewModel UserManagement { get; }
        public LibraryStatsViewModel LibraryStats { get; }
        public ProfileViewModel profileViewModel { get; }

        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
        }

        public ReactiveCommand<Unit, Unit> ToProfile { get; }
        public ReactiveCommand<Unit, Unit> ToUserManagement { get; }
        public ReactiveCommand<Unit, Unit> ToLibraryStats { get; }
        public AdminMainMenuViewModel(Admin admin, INavigationService main)
        {
            _main = main;
            UserManagement = new UserManagementViewModel(admin, _main, this);
            LibraryStats = new LibraryStatsViewModel(admin);
            profileViewModel = new ProfileViewModel(admin,_main, _main);
            CurrentViewModel = UserManagement;
            ToProfile = ReactiveCommand.Create(() => { CurrentViewModel = new ProfileViewModel(admin, _main, _main); });
            ToLibraryStats = ReactiveCommand.Create(() => { CurrentViewModel = new LibraryStatsViewModel(admin); });
            ToUserManagement = ReactiveCommand.Create(() => { CurrentViewModel = new UserManagementViewModel(admin, _main, this); });
        }
    }
}