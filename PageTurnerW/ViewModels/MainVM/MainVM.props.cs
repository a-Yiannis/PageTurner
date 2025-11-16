using System.Diagnostics;
using Common.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PageTurnerW.ViewModels;

public partial class MainVM {
	DateTime _startTime;
	readonly Stopwatch _totalStopwatch = new();
	readonly Stopwatch _lastStopwatch = new();
	Mouse.POINT _capturedMousePosition;
	
	public Mouse.POINT CapturedMousePosition => _capturedMousePosition;
	public string StartTimeText => $@"{_startTime:HH:mm}";
	public TimeSpan TotalElapsedTime => _totalStopwatch.Elapsed;
	public TimeSpan LastElapsedTime => _lastStopwatch.Elapsed;
	


	public event Action<string>? LogMessage;

	[ObservableProperty] double volume;

	[ObservableProperty] string mousePositionText = "(0, 0)";

	[ObservableProperty] string status = "Ready";

	[ObservableProperty] string statusMessage = "Standing by.";

	[ObservableProperty] bool isBusy;

	[ObservableProperty] int clicksCount;

	[ObservableProperty] double silenceThresholdS = 5;

	[ObservableProperty]
	State state = State.Idle;
}

public enum State {
	Idle,
	Capturing,
	Running,
	Paused
}
