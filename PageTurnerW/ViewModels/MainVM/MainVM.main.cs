using System.Diagnostics;
using System.Windows.Threading;
using Common.Audio;
using Common.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PageTurnerW.ViewModels;

public sealed partial class MainVM : ObservableObject,IDisposable {
	readonly DispatcherTimer _timer;
	readonly AudioWatcher _audioWatcher;
	readonly CancellationTokenSource _cts = new();

	public MainVM() {
		_timer = new DispatcherTimer {
			Interval = TimeSpan.FromMilliseconds(1000),
			IsEnabled = false
		};
		_timer.Tick += (_, _) => {
			OnPropertyChanged(nameof(TotalElapsedTime));
			OnPropertyChanged(nameof(LastElapsedTime));
		};

		_audioWatcher = new AudioWatcher();
		_audioWatcher.CheckIntervalMs = 250;
		_audioWatcher.SilenceThresholdMs = 10_000;
		SilenceThresholdS = _audioWatcher.SilenceThresholdMs / 1000.0;
		_audioWatcher.OnPeak += audioWatcherOnPeak;
		_audioWatcher.OnSilence += audioWatcherOnSilence;

		_audioWatcher.Start();
		LogMessage += message => StatusMessage = message;
	}

	void audioWatcherOnPeak(float newVolume) => Volume = newVolume;

	void audioWatcherOnSilence() {
		if (_capturedMousePosition.IsEmpty) return;
		System.Windows.Application.Current.Dispatcher.Invoke(() => {
			Click();
			LogMessage?.Invoke($"Silence detected. Clicked at X: {_capturedMousePosition}");
		});
	}

	void Click() {
		MoveMouseRandom(ref _capturedMousePosition);
		Thread.Sleep(Random.Shared.Next(100, 250));
		MoveMouseRandom(ref _capturedMousePosition);
		Mouse.LeftClick();
		_lastStopwatch.Reset();
		ClicksCount++;
	}
	
	public void Dispose() {
		_audioWatcher.OnPeak -= audioWatcherOnPeak;
		_audioWatcher.OnSilence -= audioWatcherOnSilence;
		_audioWatcher.Stop();
		_audioWatcher.Dispose();
		_timer.Stop();
		_cts.Dispose();
	}
}