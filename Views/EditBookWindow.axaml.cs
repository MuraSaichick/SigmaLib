using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SigmaLib.ViewModels;
using System.Reactive;

namespace SigmaLib;

public partial class EditBookWindow : Window
{
    public EditBookWindow()
    {
        InitializeComponent();
        this.DataContextChanged += (_, __) =>
        {
            if (DataContext is EditBookViewModel vm)
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