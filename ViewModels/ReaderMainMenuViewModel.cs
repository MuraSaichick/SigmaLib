using ReactiveUI;
using SigmaLib.Models;
using SigmaLib.Services;
using SigmaLib.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace SigmaLib.ViewModels
{
    public class ReaderMainMenuViewModel : ViewModelBase, INavigationService
    {
        private Reader _reader;
        private INavigationService _main;
        public BookSearchViewModel bookSearchViewModel { get; }
        public ProfileViewModel profileViewModel { get; }

        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
        }
        public ReactiveCommand<Unit, Unit> ToProfile { get; }
        public ReactiveCommand<Unit, Unit> ToSearchBook { get; }
        public ReaderMainMenuViewModel(INavigationService main, Reader reader)
        {
            _reader = reader;
            _main = main;
            bookSearchViewModel = new BookSearchViewModel(this, _reader, this);
            profileViewModel = new ProfileViewModel(_reader, _main, this);
            CurrentViewModel = bookSearchViewModel;
            ToProfile = ReactiveCommand.Create(() => { CurrentViewModel = profileViewModel;});
            ToSearchBook = ReactiveCommand.Create(() => { CurrentViewModel = bookSearchViewModel;});
        }
        public void NavigateTo(ViewModelBase viewModel)
        {
            CurrentViewModel = viewModel;
        }
    }
}