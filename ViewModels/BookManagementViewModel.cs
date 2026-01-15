using ReactiveUI;
using SigmaLib.Models;
using SigmaLib.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using SigmaLib.Views;
using Avalonia.Controls.ApplicationLifetimes;

namespace SigmaLib.ViewModels
{
    public class BookManagementViewModel : ViewModelBase
    {
        private readonly CatalogService _catalogService;
        private readonly ReservationSearcher _reservationSearcher;
        private Librarian _librarian;
        private readonly ReservationService _reservationService;
        private readonly INavigationService _main;
        private readonly LibrarianMainMenuViewModel _librarianMain;
        private ObservableCollection<Book> books;
        private string _selectedTitle;
        private string _selectedGenre;
        private int? _selectedYear;
        private string _selectedAuthor;
        private string _errorMessage;
        private bool _hasError;

        public ObservableCollection<Book> Books
        {
            get => books;
            set
            {
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

        public string SelectedAuthor { get => _selectedAuthor; set => this.RaiseAndSetIfChanged(ref _selectedAuthor, value); }


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

        public bool HasNoBooks => Books?.Count == 0;

        public ReactiveCommand<Unit, Unit> SearchCommand { get; }
        public ReactiveCommand<Book, Unit> AddReservationCommand { get; }
        public ReactiveCommand<Unit, Unit> CreateNewBookCommand { get; }
        public ReactiveCommand<Book, Unit> EditBookCommand { get; }
        public ReactiveCommand<Book, Unit> DeleteBookCommand { get; }
        public ReactiveCommand<Book, Unit> SetReservableStatusCommand{ get; }

        public BookManagementViewModel(INavigationService main, Librarian librarian, LibrarianMainMenuViewModel librarianMain)
        {
            _main = main;
            _librarian = librarian;
            _librarianMain = librarianMain;
            _catalogService = new CatalogService();
            _reservationService = new ReservationService();
            SearchCommand = ReactiveCommand.Create(SearchBooksByParameters);
            AddReservationCommand = ReactiveCommand.Create<Book>((book) => { });
            CreateNewBookCommand = ReactiveCommand.Create(CreateBook);
            DeleteBookCommand = ReactiveCommand.Create<Book>(DeleteBook);
            EditBookCommand = ReactiveCommand.Create<Book>(EditBook);
            SetReservableStatusCommand = ReactiveCommand.Create<Book>(SetReservableStatus);
            SearchBooksByParameters();
        }
        private void SearchBooksByParameters()
        {
            Books = new ObservableCollection<Book>(_catalogService.SearchBooks(SelectedAuthor, SelectedTitle, _selectedYear, SelectedGenre));
        }

        private void DeleteBook(Book book)
        {
            OperationResult operationResult = _catalogService.DeleteBook(book.Id);
            book.IsSuccessResultOperation = operationResult.Success;
            book.ResultOperation = operationResult.Message;
            if (book.IsSuccessResultOperation)
            {
                SearchBooksByParameters();
            }
        }
        private async void CreateBook()
        {
            var vm = new AddBookViewModel(_catalogService);
            var window = new AddBookWindow
            {
                DataContext = vm
            };
            if(App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                await window.ShowDialog(desktop.MainWindow);
                SearchBooksByParameters();
            }
        }
        private void SetReservableStatus(Book book)
        {
            _catalogService.SetReservableStatusBook(book);
        }
        private async void EditBook(Book book)
        {
            var vm = new EditBookViewModel(_catalogService, book);
            var window = new EditBookWindow
            {
                DataContext = vm
            };
            if(App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                await window.ShowDialog(desktop.MainWindow);
                SearchBooksByParameters();
            }
        }
    }
}