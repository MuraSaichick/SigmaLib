using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SigmaLib.ViewModels;
using System.Reactive;

namespace SigmaLib.Views;

public partial class EditUserWindow : Window
{
    public EditUserWindow()
    {
        InitializeComponent();
        this.DataContextChanged += (_, __) =>
        {
            if (DataContext is EditUserViewModel vm)
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