using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using SigmaLib.Models;
using SigmaLib.Services;
namespace SigmaLib.ViewModels
{
    public class CheckReaderViewModel : ViewModelBase, INavigationService
    {
        private ViewModelBase currentViewModel;
        private User currentUser;

        public User CurrentUser
        {
            get => currentUser;
            set => this.RaiseAndSetIfChanged(ref currentUser, value);
        }
        public ViewModelBase CurrentViewModel
        {
            get => currentViewModel;
            set => this.RaiseAndSetIfChanged(ref currentViewModel, value);
        }

        public CheckReaderViewModel(User user)
        {
            CurrentUser = user;
            CurrentViewModel = new ProfileViewModel(CurrentUser, this, this);
        }

        public void NavigateTo(ViewModelBase viewModel)
        {
            CurrentViewModel = viewModel;
        }
    }
}
