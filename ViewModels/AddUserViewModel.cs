using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Builder;
using SigmaLib.Models;
using SigmaLib.Services;


namespace SigmaLib.ViewModels
{
    public class AddUserViewModel : ViewModelBase
    {
        private UserService _userService;
        private User user;
        private string _resultMessage = string.Empty;
        private bool _hasResult = false;
        private string _resultColor = "#28a745"; // Зеленый 
        public User User {
            get => user;
            set => this.RaiseAndSetIfChanged(ref user, value);
        }
        public string ResultMessage
        {
            get => _resultMessage;
            set => this.RaiseAndSetIfChanged(ref _resultMessage, value);
        }

        public bool HasError
        {
            get => _hasResult;
            set => this.RaiseAndSetIfChanged(ref _hasResult, value);
        }

        public string ErrorColor
        {
            get => _resultColor;
            set => this.RaiseAndSetIfChanged(ref _resultColor, value);
        }

        public Interaction<Unit, Unit> CloseWindow { get; }
        public ReactiveCommand<Unit,Unit> AddNewUserCommand { get;}
        public ReactiveCommand<Unit,Unit> CancelCommand { get;}
        public AddUserViewModel()
        {
            User = new User() { Role = UserRole.Reader };
            _userService = new UserService();
            ErrorColor = "#dc3545"; // Красный
            CloseWindow = new Interaction<Unit, Unit>();
            AddNewUserCommand = ReactiveCommand.Create(CreateUser);
            CancelCommand = ReactiveCommand.Create(() =>
            {
                CloseWindow.Handle(Unit.Default).Subscribe();
            });

        }
        private void CreateUser()
        {
            OperationResult operationResult = new OperationResult();
            if (string.IsNullOrWhiteSpace(User.FirstName) ||
                string.IsNullOrWhiteSpace(User.LastName) ||
                string.IsNullOrWhiteSpace(User.SurName) ||
                string.IsNullOrWhiteSpace(User.Email) ||
                string.IsNullOrWhiteSpace(User.Password) ||
                string.IsNullOrWhiteSpace(User.Phone) ||
                string.IsNullOrWhiteSpace(User.Address))
            {
                ResultMessage = "Заполните все поля";
                
                HasError = true;
                Task.Delay(5000).ContinueWith(_ =>
                {
                    HasError = false;
                    ResultMessage = string.Empty;
                });
                return;
            }
            if(User.Password.Length < 8)
            {
                ResultMessage = "Пароль минимум 8 символов";
                HasError = true;
                Task.Delay(5000).ContinueWith(_ =>
                {
                    HasError = false;
                    ResultMessage = string.Empty;
                });
                return;
            }

            operationResult = _userService.CreateUser(User);
            ResultMessage = operationResult.Message;
            HasError = !operationResult.Success;
            if(HasError)
            {
                Task.Delay(5000).ContinueWith(_ =>
                {
                    HasError = false;
                    ResultMessage = string.Empty;
                });
                return;
            }
            CancelCommand.Execute().Subscribe();
        }
    }
}
