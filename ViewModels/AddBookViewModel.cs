using ReactiveUI;
using SigmaLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using SigmaLib.Services;

namespace SigmaLib.ViewModels
{
    public class AddBookViewModel : ViewModelBase
    {
        private Book _book;
        private ObservableCollection<BookCopy> bookCopies;
        private CatalogService _catalogService;
        private string errorMessage;

        public string ErrorMessage
        {
            get => errorMessage;
            set => this.RaiseAndSetIfChanged(ref errorMessage, value);
        }
        public Book NewBook { get => _book; set => _book = value;}

        public ObservableCollection<BookCopy> BookCopies
        {
            get => bookCopies;
            set => this.RaiseAndSetIfChanged(ref bookCopies, value);
        }
        public Interaction<Unit, Unit> CloseWindow { get; }

        public ReactiveCommand<Unit, Unit> CancelCommand { get; }
        public ReactiveCommand<Unit, Unit> AddBookCommand { get; }

        public AddBookViewModel(CatalogService catalogService)
        {
            _catalogService = catalogService;
            NewBook = new Book();
            this.WhenAnyValue(x => x.NewBook.TotalCopies).Subscribe(UpdateCopies);
            CloseWindow = new Interaction<Unit, Unit>();
            CancelCommand = ReactiveCommand.Create(() =>
            {
                CloseWindow.Handle(Unit.Default).Subscribe();
            });
            AddBookCommand = ReactiveCommand.Create(AddBook);
        }

        private void UpdateCopies(int total)
        {
            // если количество изменилось — пересоздаём коллекцию
            var newList = new ObservableCollection<BookCopy>();
            for (int i = 0; i < total; i++)
            {
                newList.Add(new BookCopy
                {
                    Id = i + 1,
                    Status = "Available",
                    Location = string.Empty,
                    Condition = "New"
                });
            }
            BookCopies = newList;
        }
        private void AddBook()
        {
            ErrorMessage = string.Empty;
            if (String.IsNullOrEmpty(NewBook.Title) ||
                String.IsNullOrEmpty(NewBook.Genre) ||
                String.IsNullOrEmpty(NewBook.Author))
            {
                ErrorMessage = "Заполните все поля";
                return;
            }
            if (!decimal.TryParse(NewBook.Price.ToString(), out var price)
    || !int.TryParse(NewBook.Year.ToString(), out var year)
    || year <= 0)
            {
                ErrorMessage = "Неправильный формат года или штрафа";
                return;
            }

            if(HasEmptyLocation(BookCopies))
            {
                return;
            }

            _catalogService.AddBook(_book, BookCopies.ToList<BookCopy>());

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
