using Avalonia;
using Avalonia.Controls;

namespace FactuurWizard;

public partial class InvoiceItem : UserControl
{
	public static readonly StyledProperty<string> ReferenceNumberProperty =
		AvaloniaProperty.Register<InvoiceItem, string>(
			nameof(ReferenceNumber), "");

	public static readonly StyledProperty<string> DescriptionProperty =
		AvaloniaProperty.Register<InvoiceItem, string>(
			nameof(Description), "");

	public static readonly StyledProperty<decimal> PriceProperty =
		AvaloniaProperty.Register<InvoiceItem, decimal>(
			nameof(Price));

	public string ReferenceNumber {
		get => GetValue(ReferenceNumberProperty);
		set => SetValue(ReferenceNumberProperty, value);
	}

	public string Description {
		get => GetValue(DescriptionProperty);
		set => SetValue(DescriptionProperty, value);
	}

	public decimal Price {
		get => GetValue(PriceProperty);
		set => SetValue(PriceProperty, value);
	}

	public InvoiceItem()
	{
		InitializeComponent();
		AppState.InvoiceItems.Add(this);
	}

	private void OnPriceChanged(object? sender, TextChangedEventArgs e)
	{
		var field = (sender as InvoiceField)!;

		if (!decimal.TryParse(field.Value, out _) && field.Value != "")
			field.ValueTextBox.Classes.Add("Invalid");
		else
			field.ValueTextBox.Classes.Remove("Invalid");
	}
}