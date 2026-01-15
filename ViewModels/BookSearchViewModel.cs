using ReactiveUI;
using SigmaLib.Models;
using SigmaLib.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;
namespace SigmaLib.ViewModels
{
    public class BookSearchViewModel : ViewModelBase
    {
        private readonly ReservationService _reservationService;
        private readonly INavigationService _main;
        private readonly ReaderMainMenuViewModel _readerMain;
        private Reader _reader;
        private ObservableCollection<Book> books;
        private string _selectedTitle;
        private string _selectedGenre;
        private int? _selectedYear;
        private string _selectedAuthor;
        private string _selectedIsbnNumber;
        private string _errorMessage;
        private bool _hasError;
        public ObservableCollection<Book> Books
        {
            get => books;
            set {
                this.RaiseAndSetIfChanged(ref books, value);
                this.RaisePropertyChanged(nameof(HasNoBooks));
            }
        }
        public string SelectedTitle { get => _selectedTitle; set => this.RaiseAndSetIfChanged(ref _selectedTitle, value); }
        public string SelectedGenre { get => _selectedGenre; set => this.RaiseAndSetIfChanged(ref _selectedGenre, value); }

        public string SelectedYear
        {
            get => _selectedYear?.ToString() ?? string.Empty;
            set
            {
                if (int.TryParse(value, out var year))
                {
                    this.RaiseAndSetIfChanged(ref _selectedYear, year);
                }
                else
                {
                    this.RaiseAndSetIfChanged(ref _selectedYear, null);
                }
            }
        }

        public string SelectedAuthor { get => _selectedAuthor; set=>this.RaiseAndSetIfChanged(ref _selectedAuthor, value);}

        
        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }
        public bool HasError
        {
            get => _hasError;
            set => this.RaiseAndSetIfChanged(ref _hasError, value);
        }
        public string SelectedIsbnNumber
        {
            get => _selectedIsbnNumber;
            set => this.RaiseAndSetIfChanged(ref _selectedIsbnNumber, value);
        }

        public bool HasNoBooks => Books?.Count == 0;

        public ReactiveCommand<Unit, Unit> SearchCommand { get; }
        public ReactiveCommand<Book, Unit> AddReservationCommand { get; }

        public BookSearchViewModel(INavigationService main, Reader reader, ReaderMainMenuViewModel readerMain)
        {
            _main = main;
            _readerMain = readerMain;
            _reader = reader;
            _reservationService = new ReservationService();
            Books = new ObservableCollection<Book>(reader.GetAllBooks());
            SearchCommand = ReactiveCommand.Create(SearchBooksByParameters);
            AddReservationCommand = ReactiveCommand.Create<Book>(MakeReservation);
        }

        // Метод для очистки фильтров
        public void ClearFilters()
        {
            SelectedTitle = string.Empty;
            SelectedGenre = string.Empty;
            SelectedYear = null;
            SelectedAuthor = string.Empty;
        }
        private void SearchBooksByParameters()
        {
            Books = new ObservableCollection<Book>(_reader.GetBooksByParametrs(SelectedTitle, SelectedAuthor, _selectedYear, SelectedGenre));

        }
        private void MakeReservation(Book book)
        {
            bool isPickupDateWarning = false;
            bool isReturnDateWarning = false;
            book.ResultReservation = null;
            book.PickupDateWarning = null;
            book.ReturnDateWarning = null;
            if(_reader.Dept > 0)
            {
                book.PickupDateWarning = "Нельзя забронировать книгу с долгом";
                isPickupDateWarning = true;
                return;
            }
            if (!DateTime.TryParseExact(book.PickupDateText, "dd-MM-yyyy",
        CultureInfo.InvariantCulture, DateTimeStyles.None, out var pickupDate)) {
                book.PickupDateWarning = "Неверный формат даты получения";
                isPickupDateWarning = true;
            } else if(pickupDate < DateTime.Now.Date)
            {
                book.PickupDateWarning = "Дата получения книги должна быть не ранее даты подачи бронирования";
                isPickupDateWarning= true;
            } else if (pickupDate > DateTime.Now.Date.AddDays(7))
            {
                book.PickupDateWarning = "Бронирование доступно за 7 дней до выдачи.";
                isPickupDateWarning = true;
            }
            if (!DateTime.TryParseExact(book.ReturnDateText, "dd-MM-yyyy",
        CultureInfo.InvariantCulture, DateTimeStyles.None, out var returnDate))
            {
                book.ReturnDateWarning = "Неверный формат даты возврата";
                isReturnDateWarning = true;
            }
            else if (returnDate > pickupDate.AddDays(60))
            {
                book.ReturnDateWarning = "Книгу можно взять максимум на 30 дней";
                isReturnDateWarning = true;
            }
            if (isPickupDateWarning || isReturnDateWarning)
            {
                return;
            }
            if (pickupDate > returnDate)
            {
                book.PickupDateWarning = "Дата получения не может быть позже даты возврата";
                book.ReturnDateWarning = "Дата возврата не может быть раньше даты получения";
                return;
            }
            bool result = _reservationService.CreateReservation(book.Id, _reader.Id, pickupDate, returnDate, book.Price);
            if (result)
            {
                book.ResultReservation = "Книга забронирована";
            } else
            {
                book.ResultReservation = "Не удалось забронировать книгу, возможно экземпляры разобрали";
            }
        }
    }
}