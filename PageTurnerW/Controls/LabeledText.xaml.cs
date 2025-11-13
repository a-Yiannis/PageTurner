using System.Windows;
using System.Windows.Controls;

namespace PageTurnerW.Controls;

public partial class LabeledText : UserControl {
	public static readonly DependencyProperty LabelProperty =
		DependencyProperty.Register(nameof(Label), typeof(string), typeof(LabeledText));

	public static readonly DependencyProperty TextProperty =
		DependencyProperty.Register(nameof(Text), typeof(string), typeof(LabeledText));

	public string Label {
		get => (string)GetValue(LabelProperty);
		set => SetValue(LabelProperty, value);
	}

	public string Text {
		get => (string)GetValue(TextProperty);
		set => SetValue(TextProperty, value);
	}
    
	public LabeledText() => InitializeComponent();
}