using Avalonia.Controls;
using SigmaLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SigmaLib.Services
{
    public interface INavigationService
    {
        void NavigateTo(ViewModelBase viewModel);
    }
}
