using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using SigmaLib.Models;
using SigmaLib.Services;

namespace SigmaLib.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private User _user;
        private INavigationService _main;
        private AuthService _authService;
        private string errorMessage;
        private string login;
        private string password;
        public string ErrorMessage
        {
            get => errorMessage;
            set=> this.RaiseAndSetIfChanged(ref errorMessage, value);
        }
        public string Login
        {
            get => login;
            set => this.RaiseAndSetIfChanged(ref login, value);
        }
        public string Password
        {
            get => password;
            set => this.RaiseAndSetIfChanged(ref password, value);
        }

        public ReactiveCommand<Unit, Unit> LoginCommand { get; }
        public ReactiveCommand<Unit, Unit> GoToRegistrationCommand { get; }
        public LoginViewModel(INavigationService main)
        {
            _main = main;
            _authService = new AuthService();
            LoginCommand = ReactiveCommand.Create(ExecutingLogin);
            GoToRegistrationCommand = ReactiveCommand.Create(() => { main.NavigateTo(new RegistrationViewModel(_main));});
        }

        private void ExecutingLogin()
        {
            if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Заполните все поля!!!";
                return;
            }
            (_user, ErrorMessage) = _authService.LoginUser(Login, Password);
            if(ErrorMessage == null)
            {
                LoadViewModelForUser(_user);
            }
        }
        private void LoadViewModelForUser(User user)
        {
            switch (user.Role)
            {
                case UserRole.Admin:
                    var admin = new Admin(user);
                    _main.NavigateTo(new AdminMainMenuViewModel(admin, _main));
                    break;
                case UserRole.Librarian:
                    var librarian = new Librarian(user);
                    _main.NavigateTo(new LibrarianMainMenuViewModel(librarian, _main));
                    break;
                case UserRole.Reader:
                    var reader = new Reader(user);
                    _main.NavigateTo(new ReaderMainMenuViewModel(_main, reader));
                    break;
                default:
                    break;
            }
        }
    }
}