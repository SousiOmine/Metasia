using Avalonia;
using Avalonia.Input;
using Metasia.Core.Objects;
using Metasia.Editor.Models.KeyBindings;
using Metasia.Editor.Services;
using Metasia.Editor.ViewModels.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metasia.Editor.Models.EditCommands;
using Avalonia.Layout;

namespace Metasia.Editor.ViewModels
{
    public class TimelineViewModel : ViewModelBase
    {
        /// <summary>
        /// 表示するタイムラインオブジェクト
        /// </summary>
        public TimelineObject Timeline
        {
            get => _timeline;
            set => this.RaiseAndSetIfChanged(ref _timeline, value);
        }

        /// <summary>
        /// 横方向の表示幅の倍率
        /// </summary>
        public double Frame_Per_DIP
        {
            get => _frame_per_DIP;
            set
            {
                this.RaiseAndSetIfChanged(ref _frame_per_DIP, value);
                ChangeFramePerDIP();
            } 
        }

        /// <summary>
        /// 左のレイヤーごとのボタンのViewModelら
        /// </summary>
        public ObservableCollection<LayerButtonViewModel> LayerButtons { get; } = new();

        /// <summary>
        /// 各レイヤーのViewModel
        /// </summary>
        public ObservableCollection<LayerCanvasViewModel> LayerCanvas { get; } = new();

        /// <summary>
        /// 選択しているクリップ
        /// </summary>
        public ObservableCollection<ClipViewModel> SelectClip { get; } = new();
        
        /// <summary>
        /// 現在表示しているフレーム PlayerViewModelと連動する
        /// </summary>
        public int Frame
        {
            get => playerViewModel.Frame;
            set => playerViewModel.Frame = value;
        }

        /// <summary>
        /// タイムラインのカーソルの位置
        /// </summary>
        public double CursorLeft
        {
            get => _cursorLeft;
            set => this.RaiseAndSetIfChanged(ref _cursorLeft, value);
        }

        private TimelineObject _timeline;
        private double _frame_per_DIP;

        private double _cursorLeft;

        private PlayerViewModel playerViewModel;
        private readonly IKeyBindingService _keyBindingService;

        public TimelineViewModel(PlayerViewModel playerViewModel, IKeyBindingService keyBindingService)
        {
            this.playerViewModel = playerViewModel;
            _keyBindingService = keyBindingService;

            //横方向の拡大率は初期３で固定
            Frame_Per_DIP = 3;
            _timeline = playerViewModel.TargetTimeline;

            //PlayerViewModel側からフレームの変更があればカーソルの描画位置を反映
            playerViewModel.WhenAnyValue(x => x.Frame).Subscribe
                (Frame =>
                {
                    CursorLeft = Frame * Frame_Per_DIP;
                });

            // ViewPaintRequestのハンドラを設定
            playerViewModel.ViewPaintRequest += () =>
            {
                // タイムラインの更新が必要な場合はここで行う
                CursorLeft = playerViewModel.Frame * Frame_Per_DIP;
            };

            //プロジェクトに変更が加えられたときの動作
            playerViewModel.ProjectChanged += (sender, args) =>
            {
                //レイヤーにあるクリップのサイズを再計算
                foreach (var layer in LayerCanvas)
                {
                    layer.RecalculateSize();
                }
            };

            foreach (var layer in Timeline.Layers)
            {
                LayerButtons.Add(new LayerButtonViewModel(this, layer));
                LayerCanvas.Add(new LayerCanvasViewModel(this, layer));
            }

            SelectClip.CollectionChanged += ((sender, args) =>
            {
                foreach (var layerCanvas in LayerCanvas)
                {
                    layerCanvas.ResetSelectedClip();
                }
                playerViewModel.SelectingObjects.Clear();
                foreach (var targetClip in SelectClip)
                {
                    targetClip.IsSelecting = true;
                    playerViewModel.SelectingObjects.Add(targetClip.TargetObject);
                }
            });
        }

        // コンストラクタのオーバーロード（互換性のため）
        public TimelineViewModel(PlayerViewModel playerViewModel) 
            : this(playerViewModel, App.Current?.Services?.GetService(typeof(IKeyBindingService)) as IKeyBindingService ?? 
                  throw new InvalidOperationException("KeyBindingService is not registered"))
        {
        }

        public bool RunEditCommand(IEditCommand command)
        {
            return playerViewModel.RunEditCommand(command);
        }

        public void SetFrameFromPosition(double position)
        {
            Frame = (int)(position / Frame_Per_DIP);
        }

        /// <summary>
        /// クリップを選択する
        /// </summary>
        /// <param name="clip">選択するクリップ</param>
        /// <param name="modifiers">押されていた修飾キー</param>
        public void ClipSelect(ClipViewModel clip, KeyModifiers modifiers = KeyModifiers.None)
        {
            // 複数選択用の修飾キーを取得
            var multiSelectModifiers = _keyBindingService.GetModifiers(InteractionIdentifier.MultiSelect);
            
            // 修飾キーが押されているかチェック
            bool isMultiSelectModifierPressed = (modifiers & multiSelectModifiers) == multiSelectModifiers;
            
            if (isMultiSelectModifierPressed)
            {
                // 修飾キーが押されている場合は複数選択モード
                if (SelectClip.Contains(clip))
                {
                    // すでに選択されている場合は選択解除
                    SelectClip.Remove(clip);
                }
                else
                {
                    // 選択されていない場合は追加
                    SelectClip.Add(clip);
                }
            }
            else
            {
                // 修飾キーが押されていない場合は単一選択モード
                SelectClip.Clear();
                SelectClip.Add(clip);
            }
        }

        /// <summary>
        /// 後方互換性のための単一選択メソッド
        /// </summary>
        public void ClipSelect(ClipViewModel clip)
        {
            ClipSelect(clip, KeyModifiers.None);
        }

        public bool CanResizeClip(MetasiaObject clipObject, int newStartFrame, int newEndFrame)
        {
            LayerObject? ownerLayer = FindOwnerLayer(clipObject);

            if (ownerLayer is not null)
            {
                return ownerLayer.CanPlaceObjectAt(clipObject, newStartFrame, newEndFrame);
            }
            return false;
        }

        private LayerObject? FindOwnerLayer(MetasiaObject targetObject)
        {
            foreach (var layer in Timeline.Layers)
            {
                if (layer.Objects.Any(x => x.Id == targetObject.Id))
                {
                    return layer;
                }
            }
            return null;
        }
        
        private void ChangeFramePerDIP()
        {
            CursorLeft = Frame * Frame_Per_DIP;
        }
    }
}
