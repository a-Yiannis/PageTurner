using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using Common.Audio;
using Common.Input;
using PageTurnerW.Helpers;

namespace PageTurnerW.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase {
	readonly DispatcherTimer _timer;
	readonly AudioWatcher _audioWatcher;


	DateTime _startTime;
	readonly Stopwatch _stopwatch = new();
	bool _isCapturingMouse;
	Mouse.POINT _capturedMousePosition;
	
	readonly CancellationTokenSource _cts = new();

	public Mouse.POINT CapturedMousePosition => _capturedMousePosition;

	public double VolumeProgressBarValue {
		get;
		set => SetProperty(ref field, value);
	}

	public string MousePositionText {
		get;
		set => SetProperty(ref field, value);
	} = "(0, 0)";

	public string StartTimeText => $@"{_startTime:HH:mm}";
	public TimeSpan ElapsedTime => _stopwatch.Elapsed;

	public string Status {
		get;
		set => SetProperty(ref field, value);
	} = "Ready";

	public string StatusMessage {
		get;
		set => SetProperty(ref field, value);
	} = "Standing by.";

	public int ClicksCount { get; set => SetProperty(ref field, value); } = 0;

	public event Action<string>? LogMessage;

	public MainWindowViewModel() {
		_timer = new DispatcherTimer {
			Interval = TimeSpan.FromMilliseconds(1000),
			IsEnabled = false
		};
		_timer.Tick += (_, _) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ElapsedTime)));

		_audioWatcher = new AudioWatcher(250);
		_audioWatcher.PeakMeasured += AudioWatcher_PeakMeasured;
		_audioWatcher.SilenceDetected += AudioWatcher_SilenceDetected;

		_audioWatcher.Start();
		LogMessage += message => StatusMessage = message;
	}

	void AudioWatcher_PeakMeasured(float newVolume) {
		// WASAPI reports volume from 0.0 to 1.0, convert to 0-100 for ProgressBar
		// Dispatcher.Invoke is not needed here because VolumeProgressBarValue is a bound property
		VolumeProgressBarValue = newVolume;
	}

	void AudioWatcher_SilenceDetected() {
		if (_capturedMousePosition is { X: 0, Y: 0 }) return;
		// Mouse actions need to be on the UI thread
		System.Windows.Application.Current.Dispatcher.Invoke(() => {
			Click();
			LogMessage?.Invoke(
				$"Silence detected. Clicked at X: {_capturedMousePosition.X}, Y: {_capturedMousePosition.Y}");
		});
	}

	void Click() {
		// add a little chaos to simulate reality even more
		MoveMouseRandom(ref _capturedMousePosition);
		Thread.Sleep(Random.Shared.Next(100, 250));
		MoveMouseRandom(ref _capturedMousePosition);
		Mouse.LeftClick();
		ClicksCount++;
	}

	public async Task CaptureMousePosition() {
		if (_isCapturingMouse) {
			LogMessage?.Invoke("Already capturing mouse position.");
			return;
		}

		await _cts.CancelAsync();
		_isCapturingMouse = true;
		LogMessage?.Invoke("Waiting for mouse click to capture position.");

		_capturedMousePosition = await Mouse.GetMousePositionOnNextClickAsync();

		MousePositionText = _capturedMousePosition.ToString();
		LogMessage?.Invoke($"Mouse position captured: X: {_capturedMousePosition.X}, Y: {_capturedMousePosition.Y}");
		_isCapturingMouse = false;

		if (_timer.IsEnabled) return;

		_startTime = DateTime.Now;
		_stopwatch.Start();
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartTimeText)));
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ElapsedTime)));

		_timer.Start();
	}

	static bool __isBlinking;
	public async Task ShowCoordinates() {
		try {
			if (__isBlinking) return;
		
			__isBlinking = true;
			for (int j = 0; j < 10; j++) {
				ScreenPointer.Show(_capturedMousePosition);
				await Task.Delay(450, _cts.Token);
				ScreenPointer.Hide();
				await Task.Delay(300, _cts.Token);
				if (_cts.IsCancellationRequested) return;
			}
		} catch {
			// ignored
		} finally {
			__isBlinking = false;
		}
	}


	public void OnWindowClosed() {
		_audioWatcher.Stop();
		_timer.Stop();
	}

}
