using ReactiveUI;
using SigmaLib.Models;
using SigmaLib.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace SigmaLib.ViewModels
{
    public class EditUserViewModel : ViewModelBase
    {
        private UserService _userService;
        private User currentUser;
        private User user;
        private string _resultMessage = string.Empty;
        private bool _hasResult = false;
        private string _resultColor = "#28a745"; // Зеленый 
        public User User
        {
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
        public ReactiveCommand<Unit, Unit> EditUserCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }
        public EditUserViewModel(User user)
        {
            currentUser = new User(user);
            User = new User(user);
            User.Password = String.Empty;
            _userService = new UserService();
            ErrorColor = "#dc3545"; // Красный
            CloseWindow = new Interaction<Unit, Unit>();
            EditUserCommand = ReactiveCommand.Create(EditUser);
            CancelCommand = ReactiveCommand.Create(() =>
            {
                CloseWindow.Handle(Unit.Default).Subscribe();
            });

        }
        private void EditUser()
        {
            OperationResult operationResult = new OperationResult();
            if (string.IsNullOrWhiteSpace(User.FirstName) ||
    string.IsNullOrWhiteSpace(User.LastName) ||
    string.IsNullOrWhiteSpace(User.SurName) ||
    string.IsNullOrWhiteSpace(User.Email) ||
    string.IsNullOrWhiteSpace(User.Address) ||
    string.IsNullOrWhiteSpace(User.Phone))
            {
                ResultMessage = "Заполните все поля (пароль необязатель)";
                HasError = true;
                Task.Delay(5000).ContinueWith(_ =>
                {
                    HasError = false;
                    ResultMessage = string.Empty;
                });
                return;
            }

            operationResult = _userService.EditUser(currentUser,User);
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
