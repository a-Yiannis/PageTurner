using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using PageTurnerW.Helpers;
using PageTurnerW.ViewModels;

namespace PageTurnerW.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow {
	public MainWindow() => InitializeComponent();

	async void CaptureMousePosition_Click(object sender, RoutedEventArgs e) {
		try {
			await ViewModel.CaptureMousePosition();
		} catch (Exception ex) {
			LogException(ex);
		}
	}

	public MainVM ViewModel => (MainVM)Resources["ViewModel"]!;

	void OnClosing(object? sender, CancelEventArgs e) => ScreenPointer.Close();

	protected override void OnClosed(EventArgs e) {
		ViewModel.OnWindowClosed();
		base.OnClosed(e);
	}

	async void OnMouseRightButtonDown(object? sender, MouseButtonEventArgs e) {
		try {
			await ViewModel.ShowCoordinates();
		} catch {
			// ignored
		}
	}
}