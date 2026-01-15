using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SigmaLib.Interfaces;
using ReactiveUI;


namespace SigmaLib.Models
{
    public enum UserRole
    {
        Reader,
        Librarian,
        Admin
    }
    public class User : ReactiveObject
    {
        private int id;
        private string email;
        private string password;
        private string firstName;
        private string lastName;
        private string surName;
        private string phone;
        private string address;
        private bool isBlocked;
        private UserRole role;
        private bool isSuccessResultOperation;
        private string stringResultOperation;
        private decimal dept;

        public int Id
        {
            get => id;
            set => this.RaiseAndSetIfChanged(ref id, value);
        }

        public string Email
        {
            get => email;
            set => this.RaiseAndSetIfChanged(ref email, value);
        }

        public string Password
        {
            get => password;
            set => this.RaiseAndSetIfChanged(ref password, value);
        }

        public string FirstName
        {
            get => firstName;
            set => this.RaiseAndSetIfChanged(ref firstName, value);
        }

        public string LastName
        {
            get => lastName;
            set => this.RaiseAndSetIfChanged(ref lastName, value);
        }

        public string SurName
        {
            get => surName;
            set => this.RaiseAndSetIfChanged(ref surName, value);
        }

        public string Phone
        {
            get => phone;
            set => this.RaiseAndSetIfChanged(ref phone, value);
        }

        public string Address
        {
            get => address;
            set => this.RaiseAndSetIfChanged(ref address, value);
        }

        public bool IsBlocked
        {
            get => isBlocked;
            set => this.RaiseAndSetIfChanged(ref isBlocked, value);
        }

        public UserRole Role
        {
            get => role;
            set => this.RaiseAndSetIfChanged(ref role, value);
        }

        public bool IsSuccessResultOperation
        {
            get => isSuccessResultOperation;
            set => this.RaiseAndSetIfChanged(ref isSuccessResultOperation, value);
        }
        public Decimal Dept {
            get => dept;
            set => this.RaiseAndSetIfChanged(ref dept, value);
        }

        public string FullName
        {
            get => $"{LastName} {FirstName} {SurName}";
        }

        public string StringResultOperation
        {
            get => stringResultOperation;
            set => this.RaiseAndSetIfChanged(ref stringResultOperation, value);
        }
        private IProfileFeatures features;
        public IProfileFeatures Features
        {
            get => features;
            set => this.RaiseAndSetIfChanged(ref features, value);
        }

        public User(User other)
        {
            Id = other.Id;
            Email = other.Email;
            Password = other.Password;
            FirstName = other.FirstName;
            LastName = other.LastName;
            SurName = other.SurName;
            Phone = other.Phone;
            Address = other.Address;
            IsBlocked = other.IsBlocked;
            Role = other.Role;
        }
        public User() { }
    }
}