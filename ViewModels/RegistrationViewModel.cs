using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using ReactiveUI;
using SigmaLib.Models;
using SigmaLib.Services;

namespace SigmaLib.ViewModels
{
    
    public class RegistrationViewModel : ViewModelBase
    {
        private readonly INavigationService _main;
        private readonly AuthService authService;
        private string lastName;
        private string firstName;
        private string surName;
        private string email;
        private string phone;
        private string address;
        private string password;
        private string confirmPassword;
        private string errorMessage;
        public string LastName
        {
            get => lastName;
            set => this.RaiseAndSetIfChanged(ref lastName, value);
        }
        public string FirstName
        {
            get => firstName;
            set => this.RaiseAndSetIfChanged(ref firstName, value);
        }
        public string SurName
        {
            get => surName;
            set => this.RaiseAndSetIfChanged(ref surName, value);
        }
        public string Email
        {
            get => email;
            set => this.RaiseAndSetIfChanged(ref email, value);
        }
        public string Phone
        {
            get => phone;
            set => this.RaiseAndSetIfChanged(ref phone, value);
        }

        public string Address
        {
            get => address;
            set => this.RaiseAndSetIfChanged(ref address, value);
        }
        public string Password
        {
            get => password;
            set => this.RaiseAndSetIfChanged(ref password, value);
        }
        public string ConfirmPassword
        {
            get => confirmPassword;
            set => this.RaiseAndSetIfChanged(ref confirmPassword, value);
        }
        public string ErrorMessage
        {
            get => errorMessage;
            set => this.RaiseAndSetIfChanged(ref errorMessage, value);
        }
        
        public ReactiveCommand<Unit,Unit> RegistrationCommand { get; }
        public ReactiveCommand<Unit,Unit> GoToLoginCommand { get; }
        public RegistrationViewModel(INavigationService main)
        {
            _main = main;
            authService = new AuthService();
            RegistrationCommand = ReactiveCommand.Create(ExecutingRegistration);
            GoToLoginCommand = ReactiveCommand.Create(() => { _main.NavigateTo(new LoginViewModel(_main));});
        }

        private void ExecutingRegistration()
        {
            ErrorMessage = string.Empty;
            if(string.IsNullOrWhiteSpace(lastName) ||
        string.IsNullOrWhiteSpace(firstName) ||
        string.IsNullOrWhiteSpace(surName) ||
        string.IsNullOrWhiteSpace(email) ||
        string.IsNullOrWhiteSpace(phone) ||
        string.IsNullOrWhiteSpace(address) ||
        string.IsNullOrWhiteSpace(password) ||
        string.IsNullOrWhiteSpace(confirmPassword))
            {
                ErrorMessage = "Все поля должны быть заполнены!!!";
                return;
            }
            if (Password.Length < 8)
            {
                ErrorMessage = "Минимальный размер пароля 8 символов!!!";
                return;
            }
            if (string.IsNullOrWhiteSpace(Email)
    || Email.Length < 11
    || !(Email.EndsWith("@gmail.com") || Email.EndsWith("@mail.ru")))
            {
                ErrorMessage = "Ошибка: некорректный адрес электронной почты или запрещённый домен";
                return;
            }

            if(Address.Length < 5)
            {
                ErrorMessage = "Ошибка: минимальный размер адреса 5 символов";
                return;
            }

            string digitsOnly = new string(Phone.Where(char.IsDigit).ToArray());
            if (!(Phone.StartsWith("+") && digitsOnly.Length >= 9))
            {
                ErrorMessage = "Неверный формат номера телефона (минимум 9 цифр после '+')";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Пароли не совпадают!!!";
                return;
            }
            
                ErrorMessage = authService.RegistrationUser(Email, Password, FirstName, LastName, SurName, Phone, Address);
        }
    }
}
