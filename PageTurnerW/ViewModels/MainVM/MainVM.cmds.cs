using System.Diagnostics;
using Common.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PageTurnerW.Helpers;

namespace PageTurnerW.ViewModels;

public partial class MainVM {
	bool __isCapturingMouse;
	[RelayCommand]
	public async Task CaptureMousePosition() {
		if (__isCapturingMouse) {
			LogMessage?.Invoke("Already capturing mouse position.");
			return;
		}

		await _cts.CancelAsync();
		__isCapturingMouse = true;
		State = State.Capturing;
		LogMessage?.Invoke("Waiting for mouse click to capture position.");

		_capturedMousePosition = await Mouse.GetMousePositionOnNextClickAsync();

		MousePositionText = _capturedMousePosition.ToString();
		LogMessage?.Invoke($"Mouse position captured: X: {_capturedMousePosition.X}, Y: {_capturedMousePosition.Y}");
		__isCapturingMouse = false;

		if (_timer.IsEnabled) return;

		_startTime = DateTime.Now;
		_totalStopwatch.Start();
		_lastStopwatch.Start();
		OnPropertyChanged(nameof(StartTimeText));
		OnPropertyChanged(nameof(TotalElapsedTime));
		_timer.Start();
		State = State.Running;
	}
	
	public void Stop() {
		_capturedMousePosition = new Mouse.POINT();

		Status = "Stopped";
		StatusMessage = "The timer has been stopped and the position reset.";

		MousePositionText = _capturedMousePosition.ToString();
		_totalStopwatch.Reset();
		_lastStopwatch.Reset();
		OnPropertyChanged(nameof(TotalElapsedTime));
		_timer.Stop();
		State = State.Idle;
	}

	bool __isBlinking;
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
			ScreenPointer.Hide();
			__isBlinking = false;
		}
	}

	[RelayCommand]
	public void TogglePause() {
		switch (State) {
			case State.Running:
				// Currently running, so pause
				_totalStopwatch.Stop();
				_lastStopwatch.Stop();
				_timer.Stop();
				_audioWatcher.IsPaused = true;

				Status = "Paused";
				StatusMessage = "The timer has been paused.";

				State = State.Paused;

				break;
			case State.Paused:
				// Currently paused, so resume
				_totalStopwatch.Start();
				_lastStopwatch.Start();
				_audioWatcher.IsPaused = false;

				_timer.Start();

				Status = "Running";
				StatusMessage = "The timer has resumed.";

				State = State.Running;
				break;
			case State.Idle:
			case State.Capturing:
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
		OnPropertyChanged(nameof(TotalElapsedTime)); // Update UI
	}
}
