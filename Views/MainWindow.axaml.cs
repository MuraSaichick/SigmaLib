using Avalonia.Controls;
using SigmaLib.ViewModels;
namespace SigmaLib.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}