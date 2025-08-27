using Avalonia;
using Metasia.Core.Objects;
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
using Metasia.Editor.Models.EditCommands.Commands;
using System.Diagnostics;
using Metasia.Core.Objects;

namespace Metasia.Editor.ViewModels
{
    [Serializable]
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
        /// 現在表示しているフレーム PlayerViewModelと連動する
        /// </summary>
        public int Frame
        {
            get => _frame;
            set => this.RaiseAndSetIfChanged(ref _frame, value);
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

        private int _frame;
        private double _cursorLeft;

        private readonly PlayerViewModel PlayerViewModel;

        public event EventHandler? ProjectChanged;

        public TimelineViewModel(PlayerViewModel playerViewModel)
        {
            this.PlayerViewModel = playerViewModel;

            //横方向の拡大率は初期３で固定
            Frame_Per_DIP = 3;
            _timeline = PlayerViewModel.TargetTimeline;

            //PlayerViewModel側からフレームの変更があればカーソルの描画位置を反映
            PlayerViewModel.WhenAnyValue(x => x.Frame).Subscribe
                (Frame =>
                {
                    _frame = Frame;
                    CursorLeft = Frame * Frame_Per_DIP;
                });

            this.WhenAnyValue(x => x.Frame).Subscribe
                (Frame =>
                {
                    PlayerViewModel.Frame = Frame;
                    CursorLeft = Frame * Frame_Per_DIP;
                });

            // ViewPaintRequestのハンドラを設定
            PlayerViewModel.ViewPaintRequest += () =>
            {
                // タイムラインの更新が必要な場合はここで行う
                CursorLeft = Frame * Frame_Per_DIP;
            };

            //プロジェクトに変更が加えられたときには自身のイベントも発火する
            PlayerViewModel.ProjectChanged += (sender, args) =>
            {
                ProjectChanged?.Invoke(this, EventArgs.Empty);
            };

            foreach (var layer in Timeline.Layers)
            {
                LayerButtons.Add(new LayerButtonViewModel(this, layer));
                LayerCanvas.Add(new LayerCanvasViewModel(this, PlayerViewModel, layer));
            }
        }

        public bool RunEditCommand(IEditCommand command)
        {
            return PlayerViewModel.RunEditCommand(command);
        }


        public void ClipSelect(ClipObject obj, bool isMultiSelect = false)
        {
            if (isMultiSelect)
            {
                // 複数選択モード：既に選択されている場合は選択解除、そうでなければ追加
                if (PlayerViewModel.SelectingObjects.Contains(obj))
                {
                    PlayerViewModel.SelectingObjects.Remove(obj);
                }
                else
                {
                    PlayerViewModel.SelectingObjects.Add(obj);
                }
            }
            else
            {
                // 単一選択モード：既存の選択をクリアして新しいクリップを選択
                PlayerViewModel.SelectingObjects.Clear();
                PlayerViewModel.SelectingObjects.Add(obj);
            }
        }

        public void ClipRemove(ClipObject clipObject)
        {
            LayerObject? ownerLayer = FindOwnerLayer(clipObject);

            if (ownerLayer is not null)
            {
                IEditCommand command = new ClipRemoveCommand(clipObject, ownerLayer);
                RunEditCommand(command);
            }
        }

        public bool CanResizeClip(ClipObject clipObject, int newStartFrame, int newEndFrame)
        {
            LayerObject? ownerLayer = FindOwnerLayer(clipObject);

            if (ownerLayer is not null)
            {
                return ownerLayer.CanPlaceObjectAt(clipObject, newStartFrame, newEndFrame);
            }
            return false;
        }

        private LayerObject? FindOwnerLayer(ClipObject targetObject)
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
