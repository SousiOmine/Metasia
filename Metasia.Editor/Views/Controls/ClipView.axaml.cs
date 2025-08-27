using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using DynamicData.Kernel;
using Metasia.Editor.Models.DragDropData;
using Metasia.Editor.ViewModels.Controls;
using Microsoft.Extensions.DependencyInjection;
using Metasia.Editor.Services;

namespace Metasia.Editor.Views.Controls;

public partial class ClipView : UserControl
{
    private ClipViewModel? VM
    {
        get { return this.DataContext as ClipViewModel; }

    }
    public ClipView()
    {
        InitializeComponent();
        
        this.DataContextChanged += (s, e) =>
        {
            //ViewModelが置き換えられたときの処理をいつか書く
        };
    }

    private void Clip_OnTapped(object? sender, TappedEventArgs e)
    {
        TryClipSelect(e.KeyModifiers);
    }

    private void TryClipSelect(KeyModifiers modifiers)
    {
        if (VM is null) return;
        

        // キーバインディングサービスから修飾キー設定を取得
        var keyBindingService = App.Current?.Services?.GetService<IKeyBindingService>();
        var multiSelectModifier = keyBindingService?.GetModifierForAction("MultiSelectClip");

        bool isMultiSelect = multiSelectModifier.HasValue && 
                            keyBindingService.IsModifierKeyPressed(multiSelectModifier.Value, modifiers);

        VM.ClipClick(isMultiSelect);
    }
}   
