using Avalonia.Animation;
using Avalonia.Controls.ApplicationLifetimes;
using DocumentFormat.OpenXml.Spreadsheet;
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
    public class ReaderManagementViewModel : ViewModelBase
    {
        private ObservableCollection<User> _readers;
        private User _librarian;
        private string searchFirstName;
        private string searchLastName;
        private string searchSurName;
        private string searchAddress;
        private string searchPhone;
        private string searchEmail;
        private string searchDept = "Все";
        private string searchId;
        private string errorMessageId;
        private readonly UserSearcher _userSearcher;
        public ObservableCollection<User> Readers
        {
            get => _readers;
            set => this.RaiseAndSetIfChanged(ref _readers, value);
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

        public string SearchDept
        {
            get => searchDept;
            set
            {
                this.RaiseAndSetIfChanged(ref searchDept, value);
                SearchUsers();
            }
        }

        public string SearchId
        {
            get => searchId;
            set => this.RaiseAndSetIfChanged(ref searchId, value);
        }

        public string ErrorMessageId
        {
            get => errorMessageId;
            set => this.RaiseAndSetIfChanged(ref errorMessageId, value);
        }

        public List<string> DeptOptions { get; } = new List<string>
        {
            "Все",
            "С долгом",
            "Без долга"
        };
        public static readonly Dictionary<string, string> Translations = new Dictionary<string, string>
{
    { "Все", "All" },
    { "С долгом", "WithDebt" },
    { "Без долга", "WithoutDebt" }
};

        public ReactiveCommand<User, Unit> CheckReservationsOfReaderCommand { get; }
        public ReactiveCommand<Unit, Unit> ResetFiltersCommand { get; }
        public ReactiveCommand<Unit, Unit> SearchReaderCommand { get; }
        public ReactiveCommand<User, Unit> PayFineCommand { get; }

        public ReaderManagementViewModel(User librarian)
        {
            _librarian = librarian;
            _userSearcher = new UserSearcher();
            CheckReservationsOfReaderCommand = ReactiveCommand.Create<User>((reader) => { CheckReservationsOfReader(reader); });
            ResetFiltersCommand = ReactiveCommand.Create(ResetFilters);
            SearchReaderCommand = ReactiveCommand.Create(SearchUsers);
            PayFineCommand = ReactiveCommand.Create<User>((user) => {
                _userSearcher.PayFineReader(user);
                SearchUsers();
            });
            SearchUsers();
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
            Readers = new ObservableCollection<User>(_userSearcher.SearchReaders(user, SearchId,
                Translations.ContainsKey(SearchDept) ? Translations[SearchDept] : SearchDept));
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
            SearchDept = "Все";
        }

        private async void CheckReservationsOfReader(User reader)
        {
            reader.Features = new CheckReaderFeatures();
            var vm = new CheckReaderViewModel(reader);
            var window = new CheckReaderWindow()
            {
                DataContext = vm
            };
            if (App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                await window.ShowDialog(desktop.MainWindow);
            }
        }
    }
}
