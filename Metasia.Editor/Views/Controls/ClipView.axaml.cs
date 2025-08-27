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
    
    // 追加するフィールド
    private DateTime _pointerPressedTime;
    private bool _isPotentialDrag = false;
    private const int CLICK_THRESHOLD_MS = 300;

    public ClipView()
    {
        InitializeComponent();
        
        this.DataContextChanged += (s, e) =>
        {
            //ViewModelが置き換えられたときの処理をいつか書く
        };
    }

    // PointerPressedイベントハンドラ
    private void Clip_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _pointerPressedTime = DateTime.Now;
        _isPotentialDrag = false;
        
        // 左右どちらのボタンでも処理
        var properties = e.GetCurrentPoint(this).Properties;
        if (properties.IsLeftButtonPressed || properties.IsRightButtonPressed)
        {
            // 右クリックの場合、即座に選択処理を実行
            if (properties.IsRightButtonPressed)
            {
                // 右クリック時は即座に選択処理を実行
                TryClipSelect(e.KeyModifiers, e);
            }
        }
    }

    // PointerReleasedイベントハンドラ
    private void Clip_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var pressDuration = (DateTime.Now - _pointerPressedTime).TotalMilliseconds;
        
        // 短時間クリックかつドラッグが開始されていない場合のみ選択処理
        if (pressDuration < CLICK_THRESHOLD_MS && !_isPotentialDrag)
        {
            TryClipSelect(e.KeyModifiers, e);
        }
        
        _isPotentialDrag = false;
    }

    private void TryClipSelect(KeyModifiers modifiers, PointerEventArgs? pointerEventArgs)
    {
        if (VM is null) return;

        // キーバインディングサービスから修飾キー設定を取得
        var keyBindingService = App.Current?.Services?.GetService<IKeyBindingService>();
        var multiSelectModifier = keyBindingService?.GetModifierForAction("MultiSelectClip");

        bool isMultiSelect = multiSelectModifier.HasValue && 
                            keyBindingService.IsModifierKeyPressed(multiSelectModifier.Value, modifiers);

        // クリックされた位置からフレームを計算してViewModelに通知
        if (pointerEventArgs != null)
        {
            var position = pointerEventArgs.GetCurrentPoint(this).Position;
            // クリップ内の相対位置を計算
            var relativePositionX = position.X;
            // 相対位置をフレームに変換
            var frameOffset = (int)(relativePositionX / VM.Frame_Per_DIP);
            // クリップの開始フレームにオフセットを加算して絶対フレーム位置を算出
            var targetFrame = VM.TargetObject.StartFrame + frameOffset;
            
            VM.ClipClick(isMultiSelect, targetFrame);
        }
    }
    
    // ClipViewBehaviorからのドラッグ開始通知用メソッド
    public void NotifyDragStarted()
    {
        _isPotentialDrag = true;
    }
}
