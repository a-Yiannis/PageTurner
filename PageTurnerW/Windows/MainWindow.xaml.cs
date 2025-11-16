using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using PageTurnerW.Helpers;
using PageTurnerW.ViewModels;

namespace PageTurnerW.Windows;

public partial class MainWindow : Window {
	public MainWindow() => InitializeComponent();

	void OnClosing(object sender, CancelEventArgs e) {
		if (Resources["ViewModel"] is not MainVM vm) return;
		vm.Dispose();
	}

	void Coords_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e) {
		if (Resources["ViewModel"] is not MainVM vm) return;
		vm.Stop();
	}
	void Coords_OnMouseEnter(object sender, MouseEventArgs e) {
		if (Resources["ViewModel"] is not MainVM vm) return;
		vm.CapturedMousePosition.ConditionalExecute(ScreenPointer.Show);
	}
	void Coords_OnMouseLeave(object sender, MouseEventArgs e) {
		ScreenPointer.Hide();
	}

    private void Button_Click(object sender, RoutedEventArgs e)
    {

    }
}
