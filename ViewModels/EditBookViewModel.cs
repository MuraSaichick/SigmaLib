using DocumentFormat.OpenXml.Bibliography;
using ReactiveUI;
using SigmaLib.Models;
using SigmaLib.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SigmaLib.ViewModels
{
    public class EditBookViewModel : ViewModelBase
    {
        private Book _currentBook;
        private Book _tempBook;
        private ObservableCollection<BookCopy> bookCopies;
        private ObservableCollection<BookCopy> newBookCopies;
        private CatalogService _catalogService;
        private bool isEmptyListBookCopies;

        public bool IsEmptyListBookCopies
        {
            get => isEmptyListBookCopies;
            set => this.RaiseAndSetIfChanged(ref isEmptyListBookCopies, value);
        }

        private List<int> deletedBookCopyIds;
        public Book CurrentBook { get => _currentBook; set => _currentBook = value; }
        public Book TempBook { get => _tempBook; set => _tempBook = value; }

        public ObservableCollection<BookCopy> BookCopies
        {
            get => bookCopies;
            set => this.RaiseAndSetIfChanged(ref bookCopies, value);
        }
        public ObservableCollection<BookCopy> NewBookCopies
        {
            get => newBookCopies;
            set => this.RaiseAndSetIfChanged(ref newBookCopies, value);
        }
        public Interaction<Unit, Unit> CloseWindow { get; }

        public ReactiveCommand<Unit, Unit> CancelCommand { get; }
        public ReactiveCommand<Unit, Unit> EditBookCommand { get; }
        public ReactiveCommand<Unit, Unit> AddBookCopyCommand { get; }

        public ReactiveCommand<BookCopy, Unit> DeleteOldBookCopyCommand { get; }
        public ReactiveCommand<BookCopy, Unit> DeleteNewBookCopyCommand { get; }

        public EditBookViewModel(CatalogService catalogService, Book book)
        {
            _catalogService = catalogService;
            CurrentBook = new Book(book);
            TempBook = new Book(book);
            BookCopies = new ObservableCollection<BookCopy>(_catalogService.GetBookCopies(_currentBook.Id));
            NewBookCopies = new ObservableCollection<BookCopy>();
            deletedBookCopyIds = new List<int>();

            CloseWindow = new Interaction<Unit, Unit>();
            CancelCommand = ReactiveCommand.Create(() =>
            {
                CloseWindow.Handle(Unit.Default).Subscribe();
            });
            AddBookCopyCommand = ReactiveCommand.Create(() => { NewBookCopies.Add(new BookCopy() { Status = "Available", Condition="New"}); });
            EditBookCommand = ReactiveCommand.Create(() => {
                EditBook();
            });
            DeleteOldBookCopyCommand = ReactiveCommand.Create<BookCopy>((book) =>
            {
                if(book.Status == "Available")
                {
                    deletedBookCopyIds.Add(book.Id);
                    book.IsSuccessOperationResult = true;
                    book.StringOperationResult = "Копия удалена";
                }
                else
                {
                    book.IsSuccessOperationResult = false;
                    book.StringOperationResult = "Можно удалить только свободную копию";
                }
            });
            DeleteNewBookCopyCommand = ReactiveCommand.Create<BookCopy>((book) => { NewBookCopies.Remove(book);});
            this.WhenAnyValue(vm => vm.BookCopies, vm => vm.NewBookCopies)
        .Subscribe(_ => UpdateIsEmptyListBookCopies());

            // Следим за изменением содержимого коллекций
            BookCopies.CollectionChanged += (_, __) => UpdateIsEmptyListBookCopies();
            NewBookCopies.CollectionChanged += (_, __) => UpdateIsEmptyListBookCopies();

            // Первичная инициализация
            UpdateIsEmptyListBookCopies();

        }
        private void UpdateIsEmptyListBookCopies()
        {
            IsEmptyListBookCopies = !(BookCopies?.Any() == true || NewBookCopies?.Any() == true);
        }

        private void EditBook()
        {

            if (HasEmptyLocation(BookCopies) || HasEmptyLocation(newBookCopies))
            {
                return;
            }
            _catalogService.EditCurrentBook(CurrentBook, BookCopies.ToList(), newBookCopies.ToList(), deletedBookCopyIds);
            CancelCommand.Execute().Subscribe();
        }
        private bool HasEmptyLocation(IEnumerable<BookCopy> copies)
        {
            foreach (var copy in copies)
            {
                if (string.IsNullOrWhiteSpace(copy.Location))
                {
                    copy.IsSuccessOperationResult = false;
                    copy.StringOperationResult = "Место хранения не может быть пустым";
                    return true;
                }
                copy.StringOperationResult = String.Empty;
            }
            return false;
        }

    }
}
