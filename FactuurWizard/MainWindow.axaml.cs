using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using GemBox.Spreadsheet;
using PdfDocument = GemBox.Pdf.PdfDocument;

namespace FactuurWizard;

public partial class MainWindow : Window
{
	private string? EditingFilePath;

	public MainWindow()
	{
		InitializeComponent();
		var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		OutputDirectoryTextBox.Text = path.Replace("\\", "/");
	}

	private void OnNewItemClick(object? sender, RoutedEventArgs e)
	{
		var itemCount = ItemContainer.Children.Count;
		if (itemCount == 8)
			return;

		ItemContainer.Children.Add(new InvoiceItem());
	}

	private void OnRemoveItemClick(object? sender, RoutedEventArgs e)
	{
		var itemCount = ItemContainer.Children.Count;
		if (itemCount == 0)
			return;

		ItemContainer.Children.RemoveAt(itemCount - 1);
		AppState.InvoiceItems.RemoveAt(itemCount - 1);
	}

	private void OnBrowseOutputDirectoryClicked(object sender, RoutedEventArgs e)
	{
		var storage = GetTopLevel(this)!.StorageProvider;
		var defaultDirectory = storage.TryGetFolderFromPathAsync(OutputDirectoryTextBox.Text!);
		defaultDirectory.Wait();

		var folders = storage.OpenFolderPickerAsync(
			new FolderPickerOpenOptions
			{
				Title = "Choose output folder",
				AllowMultiple = false,
				SuggestedStartLocation = defaultDirectory.Result
			});

		folders.Wait();
		if (folders.Result.Count <= 0)
			return;

		OutputDirectoryTextBox.Text = folders.Result[0].Path.AbsolutePath;
	}

	private void NewInvoice()
	{
		Title = "Nieuw factuur";
		EditingFilePath = null;

		AppState.InvoiceItems.Clear();
		ItemContainer.Children.Clear();
		foreach (var field in AppState.InvoiceFields.Values)
			field.Value = "";
	}

	private void OpenInvoice()
	{
		var files = GetTopLevel(this)!.StorageProvider.OpenFilePickerAsync(
			new FilePickerOpenOptions
			{
				Title = "Select Invoice PDF",
				AllowMultiple = false,
				FileTypeFilter =
				[
					new FilePickerFileType("PDF Files")
					{
						Patterns = ["*.pdf"]
					}
				]
			}
		);

		files.Wait();
		if (files.Result.Count <= 0)
			return;

		NewInvoice();

		var pdf = PdfDocument.Load(files.Result[0].Path.LocalPath);
		var fileSpec = pdf.EmbeddedFiles
			.FirstOrDefault(x => x.Key.ToString() == "data.json");

		if (fileSpec.Value?.EmbeddedFile is null)
			return;

		using var stream = fileSpec.Value.EmbeddedFile.OpenRead();
		var json = JsonSerializer.Deserialize<JsonElement>(stream);

		foreach (var field in json.EnumerateObject())
			if (field.Name == "Items")
				foreach (var item in field.Value.EnumerateArray())
					ItemContainer.Children.Add(new InvoiceItem
					{
						ReferenceNumber = item.GetProperty("ReferenceNumber").GetString() ?? "",
						Description = item.GetProperty("Description").GetString() ?? "",
						Price = item.GetProperty("Price").GetDecimal()
					});
			else
				AppState.InvoiceFields[field.Name].Value = field.Value.ToString();

		Title = $"Factuur {AppState.InvoiceFields["Factuurnummer"]}" +
			$"- {AppState.InvoiceFields["Naam"].Value}.pdf";

		EditingFilePath = files.Result[0].Path.AbsolutePath;
	}

	private void SaveInvoice()
	{
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
		decimal totalPrice = 0;
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
		workbook.Save("Assets/TempInvoice.pdf");

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

		using var pdf = PdfDocument.Load("Assets/TempInvoice.pdf");
		var fileSpec = pdf.EmbeddedFiles.AddEmpty("data.json").Value;

		var embeddedFile = fileSpec.EmbeddedFile!;
		embeddedFile.Size = jsonBytes.Length;

		using (var stream = embeddedFile.OpenWrite())
		{
			stream.Write(jsonBytes, 0, jsonBytes.Length);
		}

		var pdfName = $"Factuur {AppState.InvoiceFields["Factuurnummer"]}" +
			$"- {AppState.InvoiceFields["Naam"].Value}";

		var pdfPath = Path.Combine(OutputDirectoryTextBox.Text ?? "", pdfName + ".pdf");

		pdf.Save(pdfPath);
		pdf.Close();
		File.Delete("Assets/TempInvoice.pdf");

		if (EditingFilePath is not null)
			File.Delete(EditingFilePath);

		Process.Start(new ProcessStartInfo
		{
			FileName = pdfPath,
			UseShellExecute = true
		});

		Title = pdfName;
		EditingFilePath = pdfPath;
	}

	private void OnNewInvoiceClick(object? sender, RoutedEventArgs e)
	{
		NewInvoice();
	}

	private void OnOpenInvoiceClicked(object? sender, RoutedEventArgs e)
	{
		OpenInvoice();
	}

	private void OnSaveInvoiceClicked(object? sender, RoutedEventArgs e)
	{
		SaveInvoice();
	}
}