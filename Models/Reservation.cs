using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
namespace SigmaLib.Models
{


    public class Reservation : ReactiveObject
    {
        public static readonly List<string> ValidStatuses = new()
        {
        "Pending",
        "Accepted",
        "Rejected",
        "Returned"
        };
        public static readonly Dictionary<string, string> StatusTranslations = new()
        {
    { "Pending", "Ожидание" },
    { "Accepted", "Принят" },
    { "Rejected", "Отказан" },
    { "Returned", "Возвращен" }
        };
        public static readonly Dictionary<string, string> StatusTranslationsReverse =
    StatusTranslations.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        private int id;
        private DateTime? createAt;
        private DateTime? pickUpAt;
        private DateTime? returnDate;
        private DateTime? responseAt;
        private DateTime? actualReturnDate;
        private string nameBook;
        private int bookId;
        private string defaultLocation;
        private string status;
        private string reason;
        private int? respondedByLibrarianId;
        private int? readerId;
        private int bookCopyId;
        private decimal fine;
        private string errorMessage;

        public string DefaultLocation
        {
            get => defaultLocation;
            set => this.RaiseAndSetIfChanged(ref defaultLocation, value);
        }
        public int IdBook
        {
            get => bookId;
            set => this.RaiseAndSetIfChanged(ref bookId, value);
        }

        public int Id
        {
            get => id;
            set => this.RaiseAndSetIfChanged(ref id, value);
        }


        public DateTime? CreateAt
        {
            get => createAt;
            set => this.RaiseAndSetIfChanged(ref createAt, value);
        }

        public DateTime? PickUpAt
        {
            get => pickUpAt;
            set => this.RaiseAndSetIfChanged(ref pickUpAt, value);
        }

        public DateTime? ReturnDate
        {
            get => returnDate;
            set => this.RaiseAndSetIfChanged(ref returnDate, value);
        }
        public DateTime? ResponseAt
        {
            get => responseAt;
            set => this.RaiseAndSetIfChanged(ref responseAt, value);
        }

        public DateTime? ActualReturnDate
        {
            get => actualReturnDate;
            set => this.RaiseAndSetIfChanged(ref actualReturnDate, value);
        }
        public string ActualReturnDateText
        {
            get => ActualReturnDate?.ToString("dd-MM-yyyy") ?? string.Empty;
        }

        public string NameBook
        {
            get => nameBook;
            set => this.RaiseAndSetIfChanged(ref nameBook, value);
        }
        public string Status
        {
            get => status;
            set => this.RaiseAndSetIfChanged(ref status, value);
        }

        public string Reason
        {
            get => reason;
            set => this.RaiseAndSetIfChanged(ref reason, value);
        }
        public int? RespondedByLibrarianId
        {
            get => respondedByLibrarianId;
            set => this.RaiseAndSetIfChanged(ref respondedByLibrarianId, value);
        }

        public int? ReaderId
        {
            get => readerId;
            set => this.RaiseAndSetIfChanged(ref readerId, value);
        }

        public int BookCopyId
        {
            get => bookCopyId;
            set => this.RaiseAndSetIfChanged(ref bookCopyId, value);
        }

        public decimal Fine
        {
            get => fine;
            set => this.RaiseAndSetIfChanged(ref fine, value);
        }
        public string ErrorMessage
        {
            get => errorMessage;
            set => this.RaiseAndSetIfChanged(ref  errorMessage, value);
        }

        public void SetStatus(string newStatus)
        {
            if (!ValidStatuses.Contains(newStatus))
                throw new ArgumentException($"Недопустимый статус: {newStatus}");
            Status = newStatus;
        }
        public bool IsValidStatus()
        {
            return ValidStatuses.Contains(Status);
        }
    }
}