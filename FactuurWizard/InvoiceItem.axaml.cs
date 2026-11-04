using Avalonia.Controls;

namespace FactuurWizard;

public partial class InvoiceItem : UserControl
{
	public string ReferenceNumber { get; set; } = "";
	public string Description { get; set; } = "";
	public float Price { get; set; }

	public InvoiceItem()
	{
		InitializeComponent();
		AppState.InvoiceItems.Add(this);
	}

	private void OnRefernceNumberChanged(object? sender, TextChangedEventArgs e)
	{
		ReferenceNumber = (sender as InvoiceField)!.Value;
	}

	private void OnDescriptionChanged(object? sender, TextChangedEventArgs e)
	{
		Description = (sender as InvoiceField)!.Value;
	}

	private void OnPriceChanged(object? sender, TextChangedEventArgs e)
	{
		var field = (sender as InvoiceField)!;

		if (!float.TryParse(field.Value, out var price) && field.Value != "")
		{
			field.ValueTextBox.Classes.Add("Invalid");
			return;
		}

		field.ValueTextBox.Classes.Remove("Invalid");
		Price = price;
	}
}