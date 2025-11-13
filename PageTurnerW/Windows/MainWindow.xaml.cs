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
	public MainWindowViewModel ViewModel { get; set; }

	public MainWindow() {
		InitializeComponent();
		ViewModel = new MainWindowViewModel();
		DataContext = ViewModel;
		ViewModel.LogMessage += Log;
	}

	async void CaptureMousePosition_Click(object sender, RoutedEventArgs e) {
		try {
			await ViewModel.CaptureMousePosition();
		} catch (Exception ex) {
			LogException(ex);
		}
	}

	void OnClosing(object? sender, CancelEventArgs e) => ScreenPointer.Close();

	protected override void OnClosed(EventArgs e) {
		ViewModel.OnWindowClosed();
		base.OnClosed(e);
	}
	void OnMouseEnter(object sender, MouseEventArgs e) => ViewModel.CapturedMousePosition.IfNotNull(ScreenPointer.Show);
	void OnMouseLeave(object sender, MouseEventArgs e) => ScreenPointer.Hide();

	async void OnMouseRightButtonDown(object? sender, MouseButtonEventArgs e) {
		try {
			await ViewModel.ShowCoordinates();
		} catch {
			// ignored
		}
	}
}