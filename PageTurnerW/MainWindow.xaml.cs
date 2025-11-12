using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using Common.Input;
using Common.Audio;

namespace PageTurnerW;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
	readonly DispatcherTimer _timer;
	readonly AudioWatcher _audioWatcher;

	DateTime _startTime;
	bool _isCapturingMouse = false;
	Mouse.POINT _capturedMousePosition;

	public MainWindow() {
		InitializeComponent();

		_timer = new DispatcherTimer {
			Interval = TimeSpan.FromSeconds(1)
		};
		_timer.Tick += Timer_Tick;

		_audioWatcher = new AudioWatcher();
		_audioWatcher.PeakMeasured += AudioWatcher_PeakMeasured;
		_audioWatcher.SilenceDetected += AudioWatcher_SilenceDetected;

		_audioWatcher.Start();

		Status.Text = "Ready";
	}

	void Timer_Tick(object? sender, EventArgs e) {
		ElapsedTimeText.Text = (DateTime.Now - _startTime).ToString(@"hh\:mm\:ss");
	}

	void AudioWatcher_PeakMeasured(float newVolume) {
		// WASAPI reports volume from 0.0 to 1.0, convert to 0-100 for ProgressBar
		Dispatcher.Invoke(() => VolumeProgressBar.Value = newVolume * 100);
	}

	void AudioWatcher_SilenceDetected() {
		if (_capturedMousePosition.X != 0 || _capturedMousePosition.Y != 0) {
			Dispatcher.Invoke(() => {
				Mouse.SetMousePosition(_capturedMousePosition.X, _capturedMousePosition.Y);
				Mouse.LeftClick();
				Log($"Silence detected. Clicked at X: {_capturedMousePosition.X}, Y: {_capturedMousePosition.Y}");
			});
		}
	}

	async void CaptureMousePosition_Click(object sender, RoutedEventArgs e) {
		if (_isCapturingMouse) {
			Log("Already capturing mouse position.");
			return;
		}

		_isCapturingMouse = true;
		Status.Text = "Waiting for mouse click...";
		Log("Waiting for mouse click to capture position.");

		// Start a new task to wait for the mouse click
		_capturedMousePosition = await Mouse.GetMousePositionOnNextClickAsync();

		MousePositionText.Text = $"X: {_capturedMousePosition.X}, Y: {_capturedMousePosition.Y}";
		Status.Text = "Mouse position captured.";
		Log($"Mouse position captured: X: {_capturedMousePosition.X}, Y: {_capturedMousePosition.Y}");
		_isCapturingMouse = false;

		// Start the timer and set start time after first click
		if (_timer.IsEnabled) return;

		_startTime = DateTime.Now;
		StartTimeText.Text = _startTime.ToString("HH:mm:ss");
		_timer.Start();
	}

	protected override void OnClosed(EventArgs e) {
		_audioWatcher.Stop();
		_timer.Stop();
		base.OnClosed(e);
	}
}