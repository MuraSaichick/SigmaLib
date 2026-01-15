using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using SigmaLib.Models;
using SigmaLib.Services;
using SigmaLib.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace SigmaLib.ViewModels
{
    public class UserManagementViewModel : ViewModelBase
    {
        private readonly Admin _admin;
        private INavigationService _main;
        private AdminMainMenuViewModel _adminMain;
        private readonly UserService _userService;
        private readonly UserSearcher _userSearcher;
        private string searchFirstName;
        private string searchLastName;
        private string searchSurName;
        private string searchAddress;
        private string searchPhone;
        private string searchEmail;
        private string searchId;
        private string searchRole = "Все";
        private string searchBlockedStatus = "Все";
        private string errorMessageId;

        private ObservableCollection<User> users;
        public ObservableCollection<User> Users
        {
            get => users;
            set => this.RaiseAndSetIfChanged(ref users, value);
        }
        public string SearchFirstName
        {
            get => searchFirstName;
            set => this.RaiseAndSetIfChanged(ref searchFirstName, value);
        }
        public string SearchLastName
        {
            get => searchLastName;
            set => this.RaiseAndSetIfChanged(ref searchLastName, value);
        }
        public string SearchSurName
        {
            get => searchSurName;
            set => this.RaiseAndSetIfChanged(ref searchSurName, value);
        }
        public string SearchAddress
        {
            get => searchAddress;
            set => this.RaiseAndSetIfChanged(ref searchAddress, value);
        }
        public string SearchPhone
        {
            get => searchPhone;
            set => this.RaiseAndSetIfChanged(ref searchPhone, value);
        }
        public string SearchEmail
        {
            get => searchEmail;
            set => this.RaiseAndSetIfChanged(ref searchEmail, value);
        }
        public string SearchId
        {
            get => searchId;
            set => this.RaiseAndSetIfChanged(ref searchId, value);
        }
        public string SearchRole
        {
            get => searchRole;
            set
            {
                this.RaiseAndSetIfChanged(ref searchRole, value);
                SearchUsers();
            }
        }
        public string SearchBlockedStatus
        {
            get => searchBlockedStatus;
            set
            {
                this.RaiseAndSetIfChanged(ref searchBlockedStatus, value);
                SearchUsers();
            }
        }
        public string ErrorMessageId
        {
            get => errorMessageId;
            set => this.RaiseAndSetIfChanged(ref errorMessageId, value);
        }

        public ReactiveCommand<Unit, Unit> SearchUserCommand { get; }
        public ReactiveCommand<Unit, Unit> ResetFiltersCommand { get; }
        public ReactiveCommand<User, Unit> ToggleUserBlockCommand { get; }
        public ReactiveCommand<User, Unit> DeleteUserCommand { get; }
        public ReactiveCommand<Unit, Unit> AddNewUserCommand { get; }
        public ReactiveCommand<User, Unit> EditUserCommand { get; }
        

        public List<string> RoleOptions { get; } = new List<string>
        {
            "Все",
            "Читатель",
            "Библиотекарь",
            "Админ"
        };
        public List<string> BlockedStatusOptions { get; } = new List<string>
        {
            "Все",
            "Активные",
            "Заблокированные",
        };
        public static readonly Dictionary<string, string> Translations = new Dictionary<string, string>
{
    { "Все", "All" },
    { "Читатель", "Reader" },
    { "Библиотекарь", "Librarian" },
    { "Админ", "Admin" },
    { "Активные", "Active" },
    { "Заблокированные", "Blocked" }
};

        public UserManagementViewModel(Admin admin, INavigationService main, AdminMainMenuViewModel adminMain)
        {
            _admin = admin;
            _main = main;
            _adminMain = adminMain;
            _userService = new UserService();
            _userSearcher = new UserSearcher();
            Users = new ObservableCollection<User>(_userSearcher.GetAllUsers());
            AddNewUserCommand = ReactiveCommand.Create(CreateNewUser);
            
            SearchUserCommand = ReactiveCommand.Create(SearchUsers);
            ResetFiltersCommand = ReactiveCommand.Create(ResetFilters);
            ToggleUserBlockCommand = ReactiveCommand.Create<User>((user) =>
            {

                if (user.Id == _admin.Id)
                {
                    user.StringResultOperation = "Нельзя блокировать самого себя";
                    user.IsSuccessResultOperation = false;
                    return;
                }
                OperationResult operationResult = new OperationResult();
                if (user.IsBlocked)
                {
                    operationResult = _userService.UnblockUser(user.Id);
                    user.IsSuccessResultOperation = operationResult.Success;
                    user.StringResultOperation = operationResult.Message;
                    if (operationResult.Success)
                    {
                        user.IsBlocked = false;
                    }
                }
                else
                {
                    operationResult = _userService.BlockUser(user.Id);
                    user.StringResultOperation = operationResult.Message;
                    user.IsSuccessResultOperation = operationResult.Success;
                    if (operationResult.Success)
                    {
                        user.IsBlocked = true;
                    }
                }
            });

            DeleteUserCommand = ReactiveCommand.Create<User>((user) =>
            {
                if (user.Id == _admin.Id)
                {
                    user.StringResultOperation = "Нельзя удалить самого себя";
                    user.IsSuccessResultOperation = false;
                    return;
                }
                OperationResult operationResult = _userService.DeleteUser(user.Id);
                user.StringResultOperation = operationResult.Message;
                user.IsSuccessResultOperation = operationResult.Success;
                if (operationResult.Success)
                {
                    Users.Remove(user);
                }
            });
            EditUserCommand = ReactiveCommand.Create<User>(EditUser);
            SearchUserCommand = ReactiveCommand.Create(SearchUsers);
        }
        private async void CreateNewUser()
        {
            var vm = new AddUserViewModel();
            var window = new AddUserWindow()
            {
                DataContext = vm
            };
            if (App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                await window.ShowDialog(desktop.MainWindow);
                UpdateUserSearchResults();
            }
        }
        private async void EditUser(User user)
        {
            var vm = new EditUserViewModel(user);
            var window = new EditUserWindow()
            {
                DataContext = vm
            };
            if (App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                await window.ShowDialog(desktop.MainWindow);
                UpdateUserSearchResults();
            }
        }
        private void UpdateUserSearchResults()
        {
            Users = new ObservableCollection<User>(_userSearcher.GetAllUsers());
        }
        private void SearchUsers()
        {
            ErrorMessageId = string.Empty;
            if (!(int.TryParse(SearchId, out int result) || string.IsNullOrEmpty(SearchId)))
            {
                ErrorMessageId = "Укажите числовое значение";
                return;
            }
            User user = new User()
            {
                FirstName = SearchFirstName,
                Phone = SearchPhone,
                LastName = SearchLastName,
                SurName = SearchSurName,
                Address = SearchAddress,
                Email = SearchEmail,
            };
            Users = new ObservableCollection<User>(
    _userSearcher.FilterUsers(
        user,
        Translations.ContainsKey(SearchRole) ? Translations[SearchRole] : SearchRole,
        Translations.ContainsKey(SearchBlockedStatus) ? Translations[SearchBlockedStatus] : SearchBlockedStatus,
        SearchId));
        }
        private void ResetFilters()
        {
            SearchFirstName = String.Empty;
            SearchLastName = String.Empty;
            SearchSurName = String.Empty;
            SearchPhone = String.Empty;
            SearchAddress = String.Empty;
            SearchEmail = String.Empty;
            SearchId = String.Empty;
            SearchBlockedStatus = "Все";
            SearchRole = "Все";
        }
    }
}