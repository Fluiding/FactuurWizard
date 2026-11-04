using System.Collections.Generic;

namespace FactuurWizard;

public static class AppState
{
	public static List<InvoiceField> InvoiceFields { get; set; } = [];
	public static List<InvoiceItem> InvoiceItems { get; set; } = [];
}