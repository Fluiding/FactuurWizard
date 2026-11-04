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

	public string Label {
		get => GetValue(LabelProperty);
		set => SetValue(LabelProperty, value);
	}

	public bool Independant {
		get => GetValue(IndependantProperty);
		set => SetValue(IndependantProperty, value);
	}

	public string Value { get; set; } = "";

	public InvoiceField()
	{
		InitializeComponent();
		if (!Independant)
			AppState.InvoiceFields.Add(this);
	}

	private void OnValueChanged(object? sender, TextChangedEventArgs e)
	{
		Value = (sender as TextBox)!.Text ?? "";
		ValueChanged?.Invoke(this, e);
	}
}