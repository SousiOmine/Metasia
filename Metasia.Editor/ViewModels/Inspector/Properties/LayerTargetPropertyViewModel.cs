using System;
using System.Linq;
using System.Timers;
using Metasia.Core.Objects;
using Metasia.Core.Objects.Parameters;
using Metasia.Editor.Models;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;
using Metasia.Editor.Models.Interactor;
using Metasia.Editor.Models.States;
using ReactiveUI;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public class LayerTargetPropertyViewModel : ViewModelBase, IDisposable
{
    private bool _disposed = false;

    public string PropertyDisplayName
    {
        get => _propertyDisplayName;
        set => this.RaiseAndSetIfChanged(ref _propertyDisplayName, value);
    }

    public string PropertyIdentifier
    {
        get => _propertyIdentifier;
        set => this.RaiseAndSetIfChanged(ref _propertyIdentifier, value);
    }

    /// <summary>
    /// 無限レイヤー（全ての下位レイヤーを対象）にするかどうか
    /// </summary>
    public bool IsInfinite
    {
        get => _isInfinite;
        set
        {
            if (_isInfinite == value)
            {
                return;
            }

            var previous = new LayerTarget { IsInfinite = _isInfinite, LayerCount = _layerCount };
            _isInfinite = value;
            this.RaisePropertyChanged();
            this.RaisePropertyChanged(nameof(IsLayerCountEnabled));

            var current = new LayerTarget { IsInfinite = _isInfinite, LayerCount = _layerCount };
            TryValueEnter(previous, current);
        }
    }

    /// <summary>
    /// 対象レイヤー数（IsInfinite=falseの場合のみ有効）
    /// </summary>
    public int LayerCount
    {
        get => _layerCount;
        set
        {
            if (_layerCount == value)
            {
                return;
            }

            var previous = new LayerTarget { IsInfinite = _isInfinite, LayerCount = _layerCount };
            _layerCount = Math.Max(1, value);
            this.RaisePropertyChanged();

            if (!_isInfinite)
            {
                var current = new LayerTarget { IsInfinite = _isInfinite, LayerCount = _layerCount };
                TryValueEnter(previous, current);
            }
        }
    }

    /// <summary>
    /// LayerCount入力が有効かどうか
    /// </summary>
    public bool IsLayerCountEnabled => !_isInfinite;

    private string _propertyDisplayName = string.Empty;
    private string _propertyIdentifier = string.Empty;
    private bool _isInfinite = true;
    private int _layerCount = 5;
    private ISelectionState _selectionState;
    private IEditCommandManager _editCommandManager;
    private IProjectState _projectState;
    private const double _valueEnterThreshold = 0.2;

    private Timer? _valueEnterTimer;
    private bool _isValueEnteringFlag = false;
    private LayerTarget _beforeValue = new LayerTarget();
    private bool _suppressChangeEvents = false;

    public LayerTargetPropertyViewModel(
        ISelectionState selectionState,
        string propertyIdentifier,
        IEditCommandManager editCommandManager,
        IProjectState projectState,
        LayerTarget target)
    {
        _propertyDisplayName = propertyIdentifier;
        _propertyIdentifier = propertyIdentifier;
        _isInfinite = target.IsInfinite;
        _layerCount = target.IsInfinite ? 5 : target.LayerCount;
        _selectionState = selectionState;
        _editCommandManager = editCommandManager;
        _projectState = projectState;

        // Undo/Redoイベントを購読してプロパティ値の変更を検知
        _editCommandManager.CommandExecuted += OnCommandChanged;
        _editCommandManager.CommandUndone += OnCommandChanged;
        _editCommandManager.CommandRedone += OnCommandChanged;
    }

    private void OnCommandChanged(object? sender, IEditCommand command)
    {
        RefreshPropertyValue();
    }



    private void RefreshPropertyValue()
    {
        // 選択中のクリップから現在のプロパティ値を取得して表示を更新
        _suppressChangeEvents = true;

        if (_selectionState.SelectedClips.Count > 0)
        {
            // 最初の選択クリップの値を表示（複数選択の場合は最初のクリップに合わせる）
            var firstClip = _selectionState.SelectedClips[0];
            if (TimelineInteractor.TryGetLayerTargetProperty(_propertyIdentifier, firstClip, out LayerTarget? currentValue))
            {
                if (currentValue is not null)
                {
                    _isInfinite = currentValue.IsInfinite;
                    _layerCount = currentValue.IsInfinite ? 5 : currentValue.LayerCount;

                    this.RaisePropertyChanged(nameof(IsInfinite));
                    this.RaisePropertyChanged(nameof(LayerCount));
                    this.RaisePropertyChanged(nameof(IsLayerCountEnabled));
                }
            }
        }

        _suppressChangeEvents = false;
    }

    private void TryValueEnter(LayerTarget previousValue, LayerTarget currentValue)
    {
        if (_suppressChangeEvents)
        {
            return;
        }

        if (!_isValueEnteringFlag)
        {
            _beforeValue = previousValue.Clone();
            _isValueEnteringFlag = true;
        }

        EnsureValueEnterTimer();
        ValueChanging(currentValue);

        if (_valueEnterTimer is not null)
        {
            _valueEnterTimer.Stop();
            _valueEnterTimer.Start();
        }
    }

    private void EnsureValueEnterTimer()
    {
        if (_valueEnterTimer is not null)
        {
            return;
        }

        _valueEnterTimer = new Timer(_valueEnterThreshold * 1000)
        {
            AutoReset = false
        };
        _valueEnterTimer.Elapsed += (_, _) =>
        {
            var currentValue = new LayerTarget { IsInfinite = _isInfinite, LayerCount = _layerCount };
            if (!AreEqual(currentValue, _beforeValue))
            {
                UpdateLayerTargetValue(_beforeValue, currentValue);
            }

            _isValueEnteringFlag = false;
        };
    }

    private bool AreEqual(LayerTarget a, LayerTarget b)
    {
        return a.IsInfinite == b.IsInfinite && a.LayerCount == b.LayerCount;
    }

    private void ValueChanging(LayerTarget currentValue)
    {
        PreviewUpdateLayerTargetValue(_beforeValue, currentValue);
    }

    private void UpdateLayerTargetValue(LayerTarget beforeValue, LayerTarget value)
    {
        var command = CreateLayerTargetValueChangeCommand(beforeValue, value);
        if (command is not null)
        {
            _editCommandManager.Execute(command);
        }
    }

    private void PreviewUpdateLayerTargetValue(LayerTarget beforeValue, LayerTarget value)
    {
        var command = CreateLayerTargetValueChangeCommand(beforeValue, value);
        if (command is not null)
        {
            _editCommandManager.PreviewExecute(command);
        }
    }

    private IEditCommand? CreateLayerTargetValueChangeCommand(LayerTarget beforeValue, LayerTarget value)
    {
        return TimelineInteractor.CreateLayerTargetValueChangeCommand(
            _propertyIdentifier,
            beforeValue,
            value,
            _selectionState.SelectedClips);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // イベント購読を解除
                if (_editCommandManager != null)
                {
                    _editCommandManager.CommandExecuted -= OnCommandChanged;
                    _editCommandManager.CommandUndone -= OnCommandChanged;
                    _editCommandManager.CommandRedone -= OnCommandChanged;
                }

                // タイマーを破棄
                if (_valueEnterTimer != null)
                {
                    _valueEnterTimer.Stop();
                    _valueEnterTimer.Dispose();
                    _valueEnterTimer = null;
                }
            }

            _disposed = true;
        }
    }
}
