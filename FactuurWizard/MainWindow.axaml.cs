using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Interactivity;
using GemBox.Pdf;
using GemBox.Spreadsheet;

namespace FactuurWizard;

public partial class MainWindow : Window
{
	// TODO: make a good installer

	public MainWindow()
	{
		InitializeComponent();
	}

	private void OnNewItemClick(object? sender, RoutedEventArgs e)
	{
		var itemCount = ItemContainer.Children.Count;
		if (itemCount == 8)
			return;

		var item = new InvoiceItem();
		ItemContainer.Children.Add(item);
	}

	private void OnRemoveItemClick(object? sender, RoutedEventArgs e)
	{
		var itemCount = ItemContainer.Children.Count;
		if (itemCount == 0)
			return;

		ItemContainer.Children.RemoveAt(itemCount - 1);
		AppState.InvoiceItems.RemoveAt(itemCount - 1);
	}

	private void OnNewInvoiceClick(object? sender, RoutedEventArgs e)
	{
		AppState.InvoiceItems.Clear();
		foreach (var field in AppState.InvoiceFields.Values)
			field.Value = "";
	}

	private void OnOpenInvoiceClick(object? sender, RoutedEventArgs e)
	{
		throw new NotImplementedException();
	}

	private void OnSaveInvoiceClick(object? sender, RoutedEventArgs e)
	{
		SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
		var workbook = ExcelFile.Load("Assets/Invoice.ods");
		var sheet = workbook.Worksheets[0];

		sheet.Cells["F5"].Value = " " + AppState.InvoiceFields["Naam"].Value;
		sheet.Cells["F6"].Value = " " + AppState.InvoiceFields["Adres 1"].Value;
		sheet.Cells["F7"].Value = " " + AppState.InvoiceFields["Adres 2"].Value;
		sheet.Cells["F8"].Value = " " + AppState.InvoiceFields["Land"].Value;
		sheet.Cells["F9"].Value = " " + AppState.InvoiceFields["BTW Nummer"].Value;
		sheet.Cells["F10"].Value = " " + AppState.InvoiceFields["ID Nummer"].Value;
		sheet.Cells["B10"].Value = " " + AppState.InvoiceFields["Factuurnummer"].Value;
		sheet.Cells["B11"].Value = " " + AppState.InvoiceFields["Factuurdatum"].Value;
		sheet.Cells["B12"].Value = " " + AppState.InvoiceFields["Vervaldatum"].Value;
		sheet.Cells["B15"].Value = " " + AppState.InvoiceFields["Merk"].Value;
		sheet.Cells["B16"].Value = " " + AppState.InvoiceFields["Model"].Value;
		sheet.Cells["B17"].Value = " " + AppState.InvoiceFields["Chassisnummer"].Value;
		sheet.Cells["B18"].Value = " " + AppState.InvoiceFields["Modeljaar"].Value;
		sheet.Cells["F15"].Value = " " + AppState.InvoiceFields["Cylinderinhoud"].Value;
		sheet.Cells["F16"].Value = " " + AppState.InvoiceFields["Vermogen (kW)"].Value;
		sheet.Cells["F17"].Value = " " + AppState.InvoiceFields["Kilometerstand"].Value;
		sheet.Cells["F18"].Value = " " + AppState.InvoiceFields["1ste inschrijving"].Value;

		var index = 0;
		var totalPrice = 0f;
		foreach (var item in AppState.InvoiceItems)
		{
			sheet.Cells[$"A{23 + index}"].Value = " " + item.ReferenceNumber;
			sheet.Cells[$"C{23 + index}"].Value = " " + item.Description;
			sheet.Cells[$"F{23 + index}"].Value = " " + item.Price.ToString("C", new CultureInfo("nl-BE"));

			totalPrice += item.Price;
			index++;
		}

		sheet.Cells["F32"].Value = " " + totalPrice.ToString("C", new CultureInfo("nl-BE"));

		sheet.PrintOptions.HorizontalCentered = true;
		sheet.NamedRanges.SetPrintArea(sheet.Cells.GetSubrange("A1:G38"));
		workbook.Save("Assets/Invoice.pdf");

		var invoiceData = AppState.InvoiceFields.ToDictionary(
			field => field.Key, object? (field) => field.Value.Value
		);

		invoiceData["Items"] = AppState.InvoiceItems
			.Select(item => new
			{
				item.ReferenceNumber,
				item.Description,
				item.Price
			}).ToList();

		var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(invoiceData);

		ComponentInfo.SetLicense("FREE-LIMITED-KEY");
		using var pdf = PdfDocument.Load("Assets/Invoice.pdf");
		var fileSpec = pdf.EmbeddedFiles.AddEmpty("data.json").Value;

		var embeddedFile = fileSpec.EmbeddedFile!;
		embeddedFile.Size = jsonBytes.Length;

		using (var stream = embeddedFile.OpenWrite())
		{
			stream.Write(jsonBytes, 0, jsonBytes.Length);
		}

		pdf.Save(Path.Combine(SaveLocationTextBox.Text ?? "", "Invoice.pdf"));
	}
}