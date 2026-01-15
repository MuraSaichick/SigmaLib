using Avalonia.Controls;
using ReactiveUI;
using SigmaLib.Models;
using SigmaLib.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SigmaLib.ViewModels
{
    public class MainViewModel : ViewModelBase, INavigationService
    {
        private ViewModelBase _currentViewModel;
        private ViewModelBase _pastViewModel;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
        }

        public MainViewModel()
        {
            _currentViewModel = new LoginViewModel(this); 
            // new BookSearchViewModel(this, new Reader() { Id = 1}, new ReaderMainViewModel(this, new Reader()));
        }

       public void NavigateTo(ViewModelBase viewModel)
        {
            CurrentViewModel = viewModel;
        }
    }
}
