using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;

namespace Metasia.Editor.Controls;

public class InspectorNumericUpDown : TemplatedControl
{
    protected override Type StyleKeyOverride => typeof(InspectorNumericUpDown);

    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<InspectorNumericUpDown, double>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<InspectorNumericUpDown, double>(nameof(Minimum), double.MinValue);

    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<InspectorNumericUpDown, double>(nameof(Maximum), double.MaxValue);

    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public static readonly StyledProperty<double> IncrementProperty =
        AvaloniaProperty.Register<InspectorNumericUpDown, double>(nameof(Increment), 1.0);

    public double Increment
    {
        get => GetValue(IncrementProperty);
        set => SetValue(IncrementProperty, value);
    }

    public static readonly StyledProperty<string> FormatStringProperty =
        AvaloniaProperty.Register<InspectorNumericUpDown, string>(nameof(FormatString), "G");

    public string FormatString
    {
        get => GetValue(FormatStringProperty);
        set => SetValue(FormatStringProperty, value);
    }

    public static readonly StyledProperty<double> DragValuePerPixelProperty =
        AvaloniaProperty.Register<InspectorNumericUpDown, double>(nameof(DragValuePerPixel), 0.01);

    public double DragValuePerPixel
    {
        get => GetValue(DragValuePerPixelProperty);
        set => SetValue(DragValuePerPixelProperty, value);
    }

    public static readonly StyledProperty<bool> SnapToIncrementProperty =
        AvaloniaProperty.Register<InspectorNumericUpDown, bool>(nameof(SnapToIncrement));

    public bool SnapToIncrement
    {
        get => GetValue(SnapToIncrementProperty);
        set => SetValue(SnapToIncrementProperty, value);
    }

    public static readonly StyledProperty<bool> IsEditingProperty =
        AvaloniaProperty.Register<InspectorNumericUpDown, bool>(nameof(IsEditing));

    public bool IsEditing
    {
        get => GetValue(IsEditingProperty);
        set => SetValue(IsEditingProperty, value);
    }

    public static readonly DirectProperty<InspectorNumericUpDown, string> DisplayTextProperty =
        AvaloniaProperty.RegisterDirect<InspectorNumericUpDown, string>(nameof(DisplayText), o => o.DisplayText);

    public string DisplayText
    {
        get => _displayText;
        private set => SetAndRaise(DisplayTextProperty, ref _displayText, value);
    }

    public static readonly DirectProperty<InspectorNumericUpDown, string> EditTextProperty =
        AvaloniaProperty.RegisterDirect<InspectorNumericUpDown, string>(nameof(EditText), o => o.EditText, (o, v) => o.EditText = v);

    public string EditText
    {
        get => _editText;
        set => SetAndRaise(EditTextProperty, ref _editText, value);
    }
    private string _displayText = "0";
    private string _editText = string.Empty;

    private Border? _displayBorder;
    private TextBox? _editTextBox;
    private RepeatButton? _upButton;
    private RepeatButton? _downButton;

    private bool _pointerCaptured;
    private bool _dragging;
    private Point _pressPoint;
    private double _pressValue;
    private double _editStartValue;

    static InspectorNumericUpDown()
    {
        ValueProperty.Changed.AddClassHandler<InspectorNumericUpDown>((o, _) => o.OnValueChanged());
        FormatStringProperty.Changed.AddClassHandler<InspectorNumericUpDown>((o, _) => o.OnValueChanged());
        IsEditingProperty.Changed.AddClassHandler<InspectorNumericUpDown>((o, e) => o.OnIsEditingChanged(e));
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_displayBorder is not null)
        {
            _displayBorder.PointerPressed -= DisplayBorderOnPointerPressed;
            _displayBorder.PointerMoved -= DisplayBorderOnPointerMoved;
            _displayBorder.PointerReleased -= DisplayBorderOnPointerReleased;
            _displayBorder.PointerCaptureLost -= DisplayBorderOnPointerCaptureLost;
        }

        if (_editTextBox is not null)
        {
            _editTextBox.KeyDown -= EditTextBoxOnKeyDown;
            _editTextBox.LostFocus -= EditTextBoxOnLostFocus;
        }

        if (_upButton is not null)
        {
            _upButton.Click -= UpButtonOnClick;
        }

        if (_downButton is not null)
        {
            _downButton.Click -= DownButtonOnClick;
        }

        _displayBorder = e.NameScope.Find<Border>("PART_DisplayBorder");
        _editTextBox = e.NameScope.Find<TextBox>("PART_EditTextBox");
        _upButton = e.NameScope.Find<RepeatButton>("PART_UpButton");
        _downButton = e.NameScope.Find<RepeatButton>("PART_DownButton");

        if (_displayBorder is not null)
        {
            _displayBorder.PointerPressed += DisplayBorderOnPointerPressed;
            _displayBorder.PointerMoved += DisplayBorderOnPointerMoved;
            _displayBorder.PointerReleased += DisplayBorderOnPointerReleased;
            _displayBorder.PointerCaptureLost += DisplayBorderOnPointerCaptureLost;
        }

        if (_editTextBox is not null)
        {
            _editTextBox.KeyDown += EditTextBoxOnKeyDown;
            _editTextBox.LostFocus += EditTextBoxOnLostFocus;
        }

        if (_upButton is not null)
        {
            _upButton.Click += UpButtonOnClick;
        }

        if (_downButton is not null)
        {
            _downButton.Click += DownButtonOnClick;
        }

        OnValueChanged();
    }

    private void OnIsEditingChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is bool isEditing && isEditing)
        {
            _editStartValue = Value;
            EditText = Value.ToString(FormatString, CultureInfo.InvariantCulture);

            Dispatcher.UIThread.Post(() =>
            {
                _editTextBox?.Focus();
                _editTextBox?.SelectAll();
            });
        }
    }

    private void OnValueChanged()
    {
        DisplayText = Value.ToString(FormatString, CultureInfo.InvariantCulture);

        if (!IsEditing)
        {
            EditText = DisplayText;
        }
    }

    private void DisplayBorderOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (IsEditing)
        {
            return;
        }

        if (_displayBorder is null)
        {
            return;
        }

        var point = e.GetCurrentPoint(_displayBorder);
        if (!point.Properties.IsLeftButtonPressed)
        {
            return;
        }

        _pressPoint = e.GetPosition(_displayBorder);
        _pressValue = Value;
        _dragging = false;

        e.Pointer.Capture(_displayBorder);
        _pointerCaptured = e.Pointer.Captured == _displayBorder;
        e.Handled = true;
    }

    private void DisplayBorderOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_pointerCaptured || _displayBorder is null)
        {
            return;
        }

        var pos = e.GetPosition(_displayBorder);
        var dx = pos.X - _pressPoint.X;

        if (!_dragging && Math.Abs(dx) >= 3)
        {
            _dragging = true;
        }

        if (!_dragging)
        {
            return;
        }

        var newValue = _pressValue + (dx * DragValuePerPixel);
        Value = CoerceValue(newValue);
        e.Handled = true;
    }

    private void DisplayBorderOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_pointerCaptured || _displayBorder is null)
        {
            return;
        }

        var wasDragging = _dragging;

        e.Pointer.Capture(null);
        _pointerCaptured = false;

        if (!wasDragging)
        {
            IsEditing = true;
        }

        _dragging = false;
        e.Handled = true;
    }

    private void DisplayBorderOnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        _pointerCaptured = false;
        _dragging = false;
    }

    private void EditTextBoxOnLostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!IsEditing)
        {
            return;
        }

        CommitEdit();
    }

    private void EditTextBoxOnKeyDown(object? sender, KeyEventArgs e)
    {
        if (!IsEditing)
        {
            return;
        }

        if (e.Key == Key.Enter)
        {
            CommitEdit();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Escape)
        {
            Value = _editStartValue;
            IsEditing = false;
            e.Handled = true;
        }
    }

    private void CommitEdit()
    {
        if (double.TryParse(EditText, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
        {
            Value = CoerceValue(parsed);
        }

        IsEditing = false;
    }

    private void UpButtonOnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (IsEditing)
        {
            return;
        }

        Value = CoerceValue(Value + Increment);
    }

    private void DownButtonOnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (IsEditing)
        {
            return;
        }

        Value = CoerceValue(Value - Increment);
    }

    private double CoerceValue(double value)
    {
        if (double.IsNaN(value))
        {
            return Minimum;
        }

        if (double.IsPositiveInfinity(value))
        {
            return Maximum;
        }

        if (double.IsNegativeInfinity(value))
        {
            return Minimum;
        }

        if (SnapToIncrement && Increment > 0)
        {
            value = Math.Round(value / Increment) * Increment;
        }

        if (value < Minimum)
        {
            return Minimum;
        }

        if (value > Maximum)
        {
            return Maximum;
        }

        return value;
    }
}
