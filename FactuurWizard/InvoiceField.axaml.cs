using System;
using Avalonia;
using Avalonia.Controls;

namespace FactuurWizard;

public partial class InvoiceField : UserControl
{
	public static readonly StyledProperty<string> LabelProperty =
		AvaloniaProperty.Register<InvoiceField, string>(nameof(Label), "");

	public static readonly StyledProperty<bool> IndependantProperty =
		AvaloniaProperty.Register<InvoiceField, bool>(nameof(Independant));

	public event EventHandler<TextChangedEventArgs>? ValueChanged;

	public static readonly StyledProperty<string> ValueProperty =
		AvaloniaProperty.Register<InvoiceField, string>(
			nameof(Value), "");

	public string Label {
		get => GetValue(LabelProperty);
		set => SetValue(LabelProperty, value);
	}

	public bool Independant {
		get => GetValue(IndependantProperty);
		set => SetValue(IndependantProperty, value);
	}

	public string Value {
		get => GetValue(ValueProperty);
		set => SetValue(ValueProperty, value);
	}

	public InvoiceField()
	{
		InitializeComponent();
	}

	protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnAttachedToVisualTree(e);

		if (!Independant)
			AppState.InvoiceFields.Add(Label, this);
	}

	private void OnValueChanged(object? sender, TextChangedEventArgs e)
	{
		Value = (sender as TextBox)!.Text ?? "";
		ValueChanged?.Invoke(this, e);
	}
}