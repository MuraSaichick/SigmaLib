using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace SigmaLib.Models
{
    public class BookCopy : ReactiveObject
    {
        private int id;
        private int idBook;
        private string status;
        private string location;
        private string condition;
        private bool isSuccessOperationResult;
        private string stringOperationResult;

        public int Id
        {
            get => id;
            set => this.RaiseAndSetIfChanged(ref id, value);
        }

        public int IdBook
        {
            get => idBook;
            set => this.RaiseAndSetIfChanged(ref idBook, value);
        }

        public string Status
        {
            get => status;
            set => this.RaiseAndSetIfChanged(ref status, value);
        }

        public string Location
        {
            get => location;
            set => this.RaiseAndSetIfChanged(ref location, value);
        }

        public string Condition
        {
            get => condition;
            set => this.RaiseAndSetIfChanged(ref condition, value);
        }
        public bool IsSuccessOperationResult
        {
            get => isSuccessOperationResult;
            set => this.RaiseAndSetIfChanged(ref isSuccessOperationResult, value);
        }
        public string StringOperationResult
        {
            get => stringOperationResult;
            set => this.RaiseAndSetIfChanged(ref stringOperationResult, value);
        }
    }
}
