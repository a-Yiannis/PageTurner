using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Common.Input;

namespace PageTurnerW.Helpers;

public static class ScreenPointer {
	static OverlayWindow? _window;

	public static void Show(int screenX, int screenY) {
		_window ??= new OverlayWindow();
		_window.Position(screenX, screenY);
		_window.Show();
	}
	public static void Show(Mouse.POINT point) => Show(point.X, point.Y);

	public static void Hide() => _window?.Hide();

	public static void Close() {
		_window?.Close();
		_window = null;
	}

	class OverlayWindow : Window {
		const int DotSize = 10;
		const int WindowPadding = 2;

		public OverlayWindow() {
			// Window configuration
			WindowStyle = WindowStyle.None;
			AllowsTransparency = true;
			Background = Brushes.Transparent;
			Topmost = true;
			ShowInTaskbar = false;
			ResizeMode = ResizeMode.NoResize;
			ShowActivated = false;
			IsHitTestVisible = false;

			// Fixed size: just enough for the dot
			Width = DotSize + (WindowPadding * 2);
			Height = DotSize + (WindowPadding * 2);

			// Content
			var canvas = new Canvas();
			var dot = new Ellipse {
				Width = DotSize,
				Height = DotSize,
				Fill = Brushes.Red
			};
			Canvas.SetLeft(dot, WindowPadding);
			Canvas.SetTop(dot, WindowPadding);
			canvas.Children.Add(dot);
			Content = canvas;
		}

		public void Position(int screenX, int screenY) {
			// Center the dot at the specified screen coordinate
			Left = screenX - (Width / 2);
			Top = screenY - (Height / 2);
		}
	}
}
