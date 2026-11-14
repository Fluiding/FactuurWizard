using System.Collections.Generic;
using Avalonia.Controls;

namespace FactuurWizard;

public static class AppState
{
	public static Dictionary<string, InvoiceField> InvoiceFields { get; set; } = [];
	public static List<InvoiceItem> InvoiceItems { get; set; } = [];
	public static Control SaveInvoiceButton { get; set; } = null!;
}