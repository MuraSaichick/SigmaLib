using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace SigmaLib.Models
{
    public class Book : ReactiveObject
    {
        // Приватные поля
        private int _id;
        private string _title;
        private string _author;
        private int _year;
        private string _genre;
        private decimal _price;
        private int _totalCopies;
        private int _availableCopies;
        private string isbnNumber;
        private string _pickupDateText;
        private string _returnDateText;
        private string _pickupDateWarning;
        private string _returnDateWarning;
        private bool _isReservable;
        private string _resultReservation;
        private bool _isSuccesResultOperation;
        private string _resultOperation;
        private string _isbn;

        // Публичные свойства
        public int Id
        {
            get => _id;
            set => this.RaiseAndSetIfChanged(ref _id, value);
        }

        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        public string Author
        {
            get => _author;
            set => this.RaiseAndSetIfChanged(ref _author, value);
        }

        public int Year
        {
            get => _year;
            set => this.RaiseAndSetIfChanged(ref _year, value);
        }

        public string Genre
        {
            get => _genre;
            set => this.RaiseAndSetIfChanged(ref _genre, value);
        }

        public decimal Price
        {
            get => _price;
            set => this.RaiseAndSetIfChanged(ref _price, value);
        }

        public int TotalCopies
        {
            get => _totalCopies;
            set => this.RaiseAndSetIfChanged(ref _totalCopies, value);
        }

        public int AvailableCopies
        {
            get => _availableCopies;
            set => this.RaiseAndSetIfChanged(ref _availableCopies, value);
        }
        public string IsbnNumber
        {
            get => isbnNumber;
            set => this.RaiseAndSetIfChanged(ref isbnNumber, value);
        }

        public string PickupDateText
        {
            get => _pickupDateText;
            set => this.RaiseAndSetIfChanged(ref _pickupDateText, value);
        }

        public string ReturnDateText
        {
            get => _returnDateText;
            set => this.RaiseAndSetIfChanged(ref _returnDateText, value);
        }

        public string PickupDateWarning
        {
            get => _pickupDateWarning;
            set => this.RaiseAndSetIfChanged(ref _pickupDateWarning, value);
        }

        public string ReturnDateWarning
        {
            get => _returnDateWarning;
            set => this.RaiseAndSetIfChanged(ref _returnDateWarning, value);
        }

        public bool IsReservable
        {
            get => _isReservable;
            set => this.RaiseAndSetIfChanged(ref _isReservable, value);
        }

        public string ResultReservation
        {
            get => _resultReservation;
            set => this.RaiseAndSetIfChanged(ref _resultReservation, value);
        }

        public bool IsSuccessResultOperation
        {
            get => _isSuccesResultOperation;
            set => this.RaiseAndSetIfChanged(ref _isSuccesResultOperation, value);
        }

        public string ResultOperation
        {
            get => _resultOperation;
            set => this.RaiseAndSetIfChanged(ref _resultOperation, value);
        }

        public void ClearResultMessage()
        {
            ResultOperation = null;
        }

        public Book(Book other)
        {
            _id = other._id;
            _title = other._title;
            _author = other._author;
            _year = other._year;
            _genre = other._genre;
            _price = other._price;
            _totalCopies = other._totalCopies;
            _availableCopies = other._availableCopies;
            _isReservable = other._isReservable;
        }
        public Book()
        {

        }
    }
}