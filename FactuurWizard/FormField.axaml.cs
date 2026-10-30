using Avalonia;
using Avalonia.Controls;

namespace FactuurWizard;

public partial class FormField : UserControl
{
	public static readonly StyledProperty<string> LabelProperty =
		AvaloniaProperty.Register<FormField, string>(nameof(Label), "");

	public static readonly StyledProperty<string> ValueProperty =
		AvaloniaProperty.Register<FormField, string>(nameof(Value), "");

	public string Label {
		get => GetValue(LabelProperty);
		set => SetValue(LabelProperty, value);
	}

	public string Value {
		get => GetValue(ValueProperty);
		set => SetValue(ValueProperty, value);
	}

	public FormField()
	{
		InitializeComponent();
		AppState.FormFields.Add(this);
	}
}