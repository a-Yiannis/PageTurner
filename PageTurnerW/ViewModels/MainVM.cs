using System.Diagnostics;
using System.Windows.Threading;
using Common.Audio;
using Common.Input;
using PageTurnerW.Helpers;
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedMember.Global

namespace PageTurnerW.ViewModels;

public sealed class MainVM : ViewModelBase {
	readonly DispatcherTimer _timer;
	readonly AudioWatcher _audioWatcher;


	DateTime _startTime;
	readonly Stopwatch _stopwatch = new();
	bool _isCapturingMouse;
	Mouse.POINT _capturedMousePosition;

	readonly CancellationTokenSource _cts = new();

	public Mouse.POINT CapturedMousePosition => _capturedMousePosition;

	public double Volume { get; set => SetField(ref field, value); }

	public string MousePositionText { get; set => SetField(ref field, value); } = "(0, 0)";

	public string StartTimeText => $@"{_startTime:HH:mm}";
	public TimeSpan ElapsedTime => _stopwatch.Elapsed;

	public string Status { get; set => SetField(ref field, value); } = "Ready";

	public string StatusMessage { get; set => SetField(ref field, value); } = "Standing by.";

	public bool IsBusy { get; set => SetField(ref field, value); }

	public int ClicksCount { get; set => SetField(ref field, value); } = 0;

	public double SilenceThresholdS {
		get;
		set {
			_audioWatcher.SilenceThresholdMs = (int)Math.Round(value / 1000);
			SetField(ref field, value);
		}
	}

	public event Action<string>? LogMessage;

	public MainVM() {
		_timer = new DispatcherTimer {
			Interval = TimeSpan.FromMilliseconds(1000),
			IsEnabled = false
		};
		_timer.Tick += (_, _) => OnPropertyChanged(nameof(ElapsedTime));

		_audioWatcher = new AudioWatcher();
		_audioWatcher.CheckIntervalMs = 250;
		_audioWatcher.SilenceThresholdMs = 10_000;
		SilenceThresholdS = _audioWatcher.SilenceThresholdMs / 1000.0;
		_audioWatcher.OnPeak += audioWatcherOnPeak;
		_audioWatcher.OnSilence += audioWatcherOnSilence;

		_audioWatcher.Start();
		LogMessage += message => StatusMessage = message;
	}

	void audioWatcherOnPeak(float newVolume) {
		// WASAPI reports volume from 0.0 to 1.0, convert to 0-100 for ProgressBar
		// Dispatcher.Invoke is not needed here because VolumeProgressBarValue is a bound property
		Volume = newVolume;
	}

	void audioWatcherOnSilence() {
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

		// cancel other operations
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
		OnPropertyChanged(nameof(StartTimeText));
		OnPropertyChanged(nameof(ElapsedTime));

		_timer.Start();
	}

	static bool __isBlinking;
	public async Task ShowCoordinates() {
		try {
			if (__isBlinking) return;

			__isBlinking = true;
			for (int i = 0; i < 10; i++) {
				Debug.WriteLine("Show!");
				ScreenPointer.Show(_capturedMousePosition);
				await Task.Delay(450, _cts.Token);
				Debug.WriteLine("Hide!");
				ScreenPointer.Hide();
				await Task.Delay(300, _cts.Token);
				if (_cts.IsCancellationRequested) return;
			}
		} catch {
			Debug.WriteLine("Blinking failed.");
		} finally {
			__isBlinking = false;
		}
	}


	public void OnWindowClosed() {
		_audioWatcher.Stop();
		_timer.Stop();
	}
}
