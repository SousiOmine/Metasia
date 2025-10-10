using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Metasia.Editor.Views;

public partial class PlayerParentView : UserControl
{
    public PlayerParentView()
    {
        InitializeComponent();

        this.DataContextChanged += (s, e) =>
        {
            if (DataContext is not null) Console.WriteLine("PlayerParentView DataContextChanged");
        };
    }
}