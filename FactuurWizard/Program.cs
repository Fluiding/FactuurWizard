using System;
using Avalonia;
using GemBox.Pdf;
using GemBox.Spreadsheet;
using Velopack;

namespace FactuurWizard;

internal class Program
{
	// Initialization code. Don't use any Avalonia, third-party APIs or any
	// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
	// yet and stuff might break.
	[STAThread]
	public static void Main(string[] args)
	{
		VelopackApp.Build().Run();

		ComponentInfo.SetLicense("FREE-LIMITED-KEY");
		SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");

		BuildAvaloniaApp()
			.StartWithClassicDesktopLifetime(args);
	}

	// Avalonia configuration, don't remove; also used by visual designer.
	public static AppBuilder BuildAvaloniaApp()
		=> AppBuilder.Configure<App>()
			.UsePlatformDetect()
#if DEBUG
			.WithDeveloperTools()
#endif
			.LogToTrace();
}