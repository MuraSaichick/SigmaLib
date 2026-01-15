using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SigmaLib.ViewModels;
using System.Reactive;

namespace SigmaLib.Views;

public partial class AddUserWindow : Window
{
    public AddUserWindow()
    {
        InitializeComponent();
        this.DataContextChanged += (_, __) =>
        {
            if (DataContext is AddUserViewModel vm)
            {
                vm.CloseWindow.RegisterHandler(interaction =>
                {
                    this.Close();
                    interaction.SetOutput(Unit.Default);
                });
            }
        };
    }
}