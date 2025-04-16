﻿using Metasia.Core.Objects;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metasia.Editor.ViewModels.Controls
{
    public class LayerCanvasViewModel : ViewModelBase
    {
        public ObservableCollection<ClipViewModel> ClipsAndBlanks { get; set; } = new();

        public double Frame_Per_DIP
        {
            get => _frame_per_DIP;
            set => this.RaiseAndSetIfChanged(ref _frame_per_DIP, value);
        }

        public double Width
        {
            get => width;
            set => this.RaiseAndSetIfChanged(ref width, value);
        }

        private TimelineViewModel parentTimeline;

        public LayerObject TargetLayer { get; private set; }

        private double _frame_per_DIP;
        private double width;

        public LayerCanvasViewModel(TimelineViewModel parentTimeline, LayerObject targetLayer) 
        {
            this.parentTimeline = parentTimeline;
            this.TargetLayer = targetLayer;

            

            parentTimeline.WhenAnyValue(x => x.Frame_Per_DIP).Subscribe
                (Frame_Per_DIP =>
                {
                    this.Frame_Per_DIP = Frame_Per_DIP;
                    ChangeFramePerDIP();
                });

            foreach(var obj in targetLayer.Objects)
            {
                var clipvm = new ClipViewModel(obj, parentTimeline);
                ClipsAndBlanks.Add(clipvm);
                
            }

            ChangeFramePerDIP();
        }

        public void ResetSelectedClip()
        {
            foreach (var clip in ClipsAndBlanks)
            {
                clip.IsSelecting = false;
            }
        }

        /// <summary>
        /// レイヤーにあるクリップの大きさを再計算する
        /// </summary>
        public void RecalculateSize()
        {
            foreach (var clip in ClipsAndBlanks)
            {
                clip.RecalculateSize();
            }
        }

        public void ClipDropped(ClipViewModel clipVM, int targetStartFrame)
        {
            parentTimeline.ClipDropped(clipVM, this, targetStartFrame);
        }

        private void ChangeFramePerDIP()
        {
            foreach (var clip in ClipsAndBlanks)
            {
                clip.Frame_Per_DIP = Frame_Per_DIP;
            }
            Width = TargetLayer.EndFrame * Frame_Per_DIP;
        }
    }
}
