using PageTurnerW.Helpers;
using PageTurnerW.Windows;
using System; // Added for Exception
using System.Configuration;
using System.Data;
using System.Diagnostics; // Added for Debug.WriteLine
using System.Windows;

namespace PageTurnerW;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application {
    readonly MainWindow? _window;
    public App() {
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        _window = new MainWindow();
        _window.Show();
        _window.Activate();
    }

    void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
        // Log the exception
        Debug.WriteLine($"Unhandled exception: {e.Exception.Message}");
        Debug.WriteLine($"Stack Trace: {e.Exception.StackTrace}");

        // Optionally, show a message box to the user
        MessageBox.Show($"An unhandled exception occurred: {e.Exception.Message}\n\nFor more details, check the debug output.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

        // Prevent the application from crashing
        e.Handled = true;
    }
}
