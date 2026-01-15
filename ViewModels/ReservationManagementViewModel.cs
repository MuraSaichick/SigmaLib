using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using SigmaLib.Interfaces;
using SigmaLib.Models;
using SigmaLib.Services;
using SigmaLib.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace SigmaLib.ViewModels
{
    public class ReservationManagementViewModel : ViewModelBase
    {
        private Librarian _librarian;
        private Reservation reservationTemplate;
        private IReservationManagementService _reservationManagementService;
        private IReservationQueryService _reservationSearcher;
        private UserSearcher _userSearcher;
        private WordDocumentService _wordDocumentService;
        private CatalogService _catalogService;
        private string _searchText = string.Empty;
        private string _searchCreateDate = string.Empty;
        private string _searchResponseDate = string.Empty;
        private string _searchReturnDate = string.Empty;
        public string _searchPickUpDate = string.Empty;
        private string _selectedStatus = "Все";
        private string _readerIdSearch = string.Empty;
        private string errorMessageReaderId;
        private string errorMessageCreateDate;
        private string errorMessagePickUpDate;
        private string errorMessageResponseDate;
        private string errorMessageReturnDate;
        private Reservation? _selectedReservation;
        private ObservableCollection<Reservation> _reservations = new();
        private ObservableCollection<Reservation> _filteredReservations = new();
     

        public ObservableCollection<Reservation> Reservations
        {
            get => _reservations;
            set => this.RaiseAndSetIfChanged(ref _reservations, value);
        }
        public Reservation ReservationTemplate
        {
            get => reservationTemplate;
            set => this.RaiseAndSetIfChanged(ref reservationTemplate, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => this.RaiseAndSetIfChanged(ref _searchText, value);
        }

        public string SearchResponseDate
        {
            get => _searchResponseDate;
            set => this.RaiseAndSetIfChanged(ref _searchResponseDate, value);
        }
        public string SearchReturnDate
        {
            get => _searchReturnDate;
            set => this.RaiseAndSetIfChanged(ref _searchReturnDate, value);
        }
        public string SearchCreateDate
        {
            get => _searchCreateDate;
            set => this.RaiseAndSetIfChanged(ref _searchCreateDate, value);
        }

        public string SearchPickUpDate
        {
            get => _searchPickUpDate;
            set => this.RaiseAndSetIfChanged(ref _searchPickUpDate, value);
        }

        public string SelectedStatus
        {
            get => _selectedStatus;
            set => this.RaiseAndSetIfChanged(ref _selectedStatus, value);
        }

        public string ReaderIdSearch
        {
            get => _readerIdSearch;
            set => this.RaiseAndSetIfChanged(ref _readerIdSearch, value);
        }

        public string ErrorMessageReaderId
        {
            get => errorMessageReaderId;
            set => this.RaiseAndSetIfChanged(ref errorMessageReaderId, value);
        }
        public string ErrorMessageCreateDate
        {
            get => errorMessageCreateDate;
            set => this.RaiseAndSetIfChanged(ref errorMessageCreateDate, value);
        }
        public string ErrorMessageResponseDate
        {
            get => errorMessageResponseDate;
            set => this.RaiseAndSetIfChanged(ref errorMessageResponseDate, value);
        }
        public string ErrorMessageReturnDate
        {
            get => errorMessageReturnDate;
            set => this.RaiseAndSetIfChanged(ref errorMessageReturnDate, value);
        }
        public string ErrorMessagePickUpDate
        {
            get => errorMessagePickUpDate;
            set => this.RaiseAndSetIfChanged(ref errorMessagePickUpDate, value);
        }

        public Reservation? SelectedReservation
        {
            get => _selectedReservation;
            set => this.RaiseAndSetIfChanged(ref _selectedReservation, value);
        }

        public ObservableCollection<Reservation> FilteredReservations
        {
            get => _filteredReservations;
            set => this.RaiseAndSetIfChanged(ref _filteredReservations, value);
        }

        public List<string> StatusOptions { get; } = new List<string>
        {
            "Все",
            "Ожидание",
            "Принят",
            "Отказан",
            "Просроченные"
        };

        public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; }
        public ReactiveCommand<Reservation, Unit> ConfirmReservationCommand { get; }
        public ReactiveCommand<Reservation, Unit> RejectReservationCommand { get; }
        public ReactiveCommand<Reservation, Unit> CheckReaderCommand { get; }
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
        public ReactiveCommand<Unit, Unit> SearchCommand { get; }
        public ReactiveCommand<Reservation, Unit> ReturnBookCommand { get; }
        public ReservationManagementViewModel(Librarian librarian)
        {
            _librarian = librarian;
            _reservationManagementService = new ReservationService();
            _reservationSearcher = new ReservationSearcher();
            _wordDocumentService = new WordDocumentService();
            _userSearcher = new UserSearcher();
            _catalogService = new CatalogService();
            Reservations = new ObservableCollection<Reservation>(_reservationSearcher.GetAllReservation());
            ReservationTemplate = new Reservation();
            ConfirmReservationCommand = ReactiveCommand.Create<Reservation>((reservation) =>
            {
                ConfirmReservation(reservation);
            });
            RejectReservationCommand = ReactiveCommand.Create<Reservation>((reservation) =>
            {
                RejectReservation(reservation);
            });
            CheckReaderCommand = ReactiveCommand.Create<Reservation>((reservation) => { CheckReader(reservation); });

            SearchCommand = ReactiveCommand.Create(SearchReservationByParametrs);
            ClearFiltersCommand = ReactiveCommand.Create(ClearFilter);
            RefreshCommand = ReactiveCommand.Create(RefreshResults);

            Reservations = new ObservableCollection<Reservation>(_reservationSearcher.GetAllReservation());
            FilteredReservations = new ObservableCollection<Reservation>(_reservationSearcher.GetAllReservation());

            ReturnBookCommand = ReactiveCommand.Create<Reservation>((reservation) => { ReturnBook(reservation); });
        }
        private void SearchReservationByParametrs()
        {
            ErrorMessageCreateDate = string.Empty;
            ErrorMessageResponseDate = string.Empty;
            ErrorMessagePickUpDate = string.Empty;
            ErrorMessageReturnDate = string.Empty;
            ErrorMessageReaderId = string.Empty;

            ReservationTemplate.NameBook = SearchText;
            ReservationTemplate.Status = SelectedStatus;
            if (string.IsNullOrWhiteSpace(ReaderIdSearch))
            {
                ReservationTemplate.ReaderId = null;
            }
            else if (int.TryParse(ReaderIdSearch, out int number))
            {
                ReservationTemplate.ReaderId = number;
                ErrorMessageReaderId = null;
            }
            else
            {
                ReservationTemplate.ReaderId = null;
                ErrorMessageReaderId = "Ошибка: строка не является числом";
            }

            if (!string.IsNullOrWhiteSpace(SearchCreateDate))
            {
                if (DateTime.TryParseExact(
                    SearchCreateDate,
                    "dd-MM-yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime createDate))
                {
                    ReservationTemplate.CreateAt = createDate;
                }
                else
                {
                    ErrorMessageCreateDate = "Ошибка: неправильный формат даты создания";
                }

            } else
            {
                ReservationTemplate.CreateAt = null;
            }
            if (!string.IsNullOrWhiteSpace(SearchPickUpDate))
            {
                if (DateTime.TryParseExact(SearchPickUpDate, "dd-MM-yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime parsedDate))
                {
                    ReservationTemplate.PickUpAt = parsedDate;
                }
                else
                {
                    ErrorMessagePickUpDate = "Ошибка: неправильный формат даты обработки";
                }
            }
            else
            {
                ReservationTemplate.PickUpAt = null;
            }
            if (!string.IsNullOrWhiteSpace(SearchReturnDate))
            {
                if (DateTime.TryParseExact(
                    SearchReturnDate,
                    "dd-MM-yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime returnDate))
                {
                    ReservationTemplate.ReturnDate = returnDate;
                }
                else
                {
                    ErrorMessageReturnDate = "Ошибка: неправильный формат даты возврата";
                }
            }
            else
            {
                ReservationTemplate.ReturnDate = null;
            }
            if (!string.IsNullOrWhiteSpace(SearchResponseDate))
            {
                if (DateTime.TryParseExact(SearchResponseDate, "dd-MM-yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime parsedDate))
                {
                    ReservationTemplate.ResponseAt = parsedDate;
                }
                else
                {
                    ErrorMessageResponseDate = "Ошибка: неправильный формат даты обработки";
                }
            }
            else
            {
                ReservationTemplate.ResponseAt = null;
            }
            FilteredReservations = new ObservableCollection<Reservation>(_reservationSearcher.GetReservationByFilter(ReservationTemplate));
        }
        private void ClearFilter()
        {
            ReservationTemplate = new Reservation();
            SearchCreateDate = string.Empty;
            SearchPickUpDate = string.Empty;
            SearchResponseDate = string.Empty;
            SearchReturnDate = string.Empty;
            SearchText = string.Empty;
            ReaderIdSearch = string.Empty;
            SelectedStatus = "Все";
            SearchReservationByParametrs();
        }
        private void RefreshResults()
        {
            SearchReservationByParametrs();
        }
        private async void CheckReader(Reservation reservation)
        {

            User user = _userSearcher.SearchById(reservation.ReaderId);
            user.Features = new CheckReaderFeatures();
            var vm = new CheckReaderViewModel(user);
            var window = new CheckReaderWindow()
            {
                DataContext = vm
            };
            if (App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                await window.ShowDialog(desktop.MainWindow);
            }
        }
        private void RejectReservation(Reservation reservation)
        {
            if(String.IsNullOrEmpty(reservation.Reason))
            {
                reservation.ErrorMessage = "Заполните причину отказа!!!";
                return;
            }
            _reservationManagementService.RejectReservation(reservation, _librarian.Id);
        }

        // Accept книги
        private void ConfirmReservation(Reservation reservation)
        {
            User reader = _userSearcher.SearchById(reservation.ReaderId);
            Book book = _catalogService.GetBookByBookCopyId(reservation.BookCopyId);
            BookCopy bookCopy = _catalogService.GetBookCopyById(reservation.BookCopyId);
            _wordDocumentService.CreateIssueContract(reservation, _librarian, reader, book, bookCopy);
            _reservationManagementService.AcceptReservation(reservation, _librarian.Id);
            RefreshResults();
        }

        private void ReturnBook(Reservation reservation)
        {
            User reader = _userSearcher.SearchById(reservation.ReaderId);
            Book book = _catalogService.GetBookByBookCopyId(reservation.BookCopyId);
            OperationResult operationResult = _reservationManagementService.ProcessReturnBook(reservation, reader, _librarian,book);
            reservation.ErrorMessage = operationResult.Message;
            
            
        }
    }
}