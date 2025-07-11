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
    /// <summary>
    /// Initializes a new instance of the <see cref="ClipView"/> control and sets up a handler for DataContext changes.
    /// </summary>
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
        if (VM is null) return;
        
        // キーバインディングサービスから修飾キー設定を取得
        var keyBindingService = App.Current?.Services?.GetService<IKeyBindingService>();
        var multiSelectModifier = keyBindingService?.GetModifierForAction("MultiSelectClip");
        
        // 現在の修飾キーの状態を取得
        var currentModifiers = e.KeyModifiers;
        
        // 複数選択モードかどうかを判定
        bool isMultiSelect = multiSelectModifier.HasValue && 
                            keyBindingService.IsModifierKeyPressed(multiSelectModifier.Value, currentModifiers);
        
        // ViewModelにクリック情報を渡す（複数選択モードかどうかも含めて）
        VM.ClipClick(isMultiSelect);
    }

    private void Handle_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        //ViewModelがnullか、Borderからのイベントであれば何もしない
        if (VM is null || sender is not Border handle) return;
        if (handle.Name != "StartHandle" && handle.Name != "EndHandle")
        {
            return;
        }

        var parentCanvas = this.Parent as Control;
        var position = e.GetCurrentPoint(parentCanvas).Position;

        //ViewModelでドラッグ開始処理
        VM.StartDrag(handle.Name, position.X);

        e.Pointer.Capture(handle);

        e.Handled = true;
    }

    private void Handle_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (VM is null || sender is not Border handle) return;
        if (e.Pointer.Captured == handle)
        {
            var parentCanvas = this.Parent as Control;
            if (parentCanvas is null) return;

            var position = e.GetCurrentPoint(parentCanvas).Position;

            VM.UpdateDrag(position.X);
            e.Handled = true;
        }
    }

    private void Handle_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (VM is null || sender is not Border handle) return;
        if (e.Pointer.Captured == handle)
        {
            var parentCanvas = this.Parent as Control;
            if (parentCanvas is null) return;

            var position = e.GetCurrentPoint(parentCanvas).Position;
            VM.EndDrag(position.X);

            e.Handled = true;
        }
        e.Pointer.Capture(null);
    }
}