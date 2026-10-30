using SixLabors.Fonts;

var collection = new FontCollection();

var family = collection.Add(
	"../../../../FactuurWizard/Assets/Fonts/AtlassianSans-latin.woff2"
);

Console.WriteLine($"Family name: {family.Name}");

foreach (var style in family.GetAvailableStyles().Span)
{
	Console.WriteLine($"Style: {style}");
}