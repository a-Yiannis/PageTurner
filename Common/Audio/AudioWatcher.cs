using System.Diagnostics;

namespace Common.Audio;

/// <summary> Continuously monitors system audio and triggers a callback after a period of silence. </summary>
public class AudioWatcher : IDisposable {
	Thread? _thread;
	volatile bool _isRunning;
	readonly Lock _syncLock = new();
	readonly Stopwatch _silenceStopwatch = Stopwatch.StartNew();

	int _checkIntervalMs = 200;

	/// <summary>
	/// Controls how often the sound volume check is performed (milliseconds).
	/// </summary>
	public int CheckIntervalMs {
		get {
			lock (_syncLock) return _checkIntervalMs;
		}
		set {
			lock (_syncLock) _checkIntervalMs = value;
		}
	}

	float _volumeThreshold = 0.05f;

	/// <summary>
	/// How loud can it be before it is considered silence.
	/// </summary>
	public float VolumeThreshold {
		get {
			lock (_syncLock) return _volumeThreshold;
		}
		set {
			lock (_syncLock) _volumeThreshold = value;
		}
	}

	bool _isPaused;

	public bool IsPaused {
		get {
			lock (_syncLock) return _isPaused;
		}
		set {
			lock (_syncLock) {
				if (value)
					_silenceStopwatch.Reset();
				_isPaused = value;
			}
		}
	}

	int _silenceThresholdMs = 5000;

	/// <summary>
	/// The minimum milliseconds that volume should be under threshold before Silence event is raised.
	/// </summary>
	public int SilenceThresholdMs {
		get {
			lock (_syncLock) return _silenceThresholdMs;
		}
		set {
			lock (_syncLock) _silenceThresholdMs = value;
		}
	}
	
	/// <summary>
	/// Provides access to the silence detection stopwatch for monitoring elapsed time.
	/// </summary>
	public TimeSpan SilenceElapsed => _silenceStopwatch.Elapsed;

	/// <summary> Raised with the current peak level on each check. </summary>
	public event Action<float>? OnPeak;

	/// <summary> Raised when the required silence duration is detected. </summary>
	public event Action? OnSilence;
	
	public event Action<Exception>? OnError;

	/// <summary>
	/// Creates a new thread and starts running the primary loop.
	/// </summary>
	public void Start() {
		if (_isRunning) return;
		_isRunning = true;
		_thread = new Thread(Run) { IsBackground = true };
		_thread.Start();
	}

	public void Stop() {
		_isRunning = false;
		_thread?.Join();
	}

	void Run() {
		try {
			while (_isRunning) {
				int checkInterval;
				lock (_syncLock) {
					if (_isPaused) {
						Thread.Sleep(PauseCheckIntervalMs);
						continue;
					}
					checkInterval = _checkIntervalMs;
				}

				float peak = WASAPI.GetSystemPeakValue();
				OnPeak?.Invoke(peak);

				lock (_syncLock) {
					if (peak < _volumeThreshold) {
						if (!_silenceStopwatch.IsRunning)
							_silenceStopwatch.Restart();

						if (_silenceStopwatch.ElapsedMilliseconds >= _silenceThresholdMs) {
							OnSilence?.Invoke();
							_silenceStopwatch.Reset();
							Thread.Sleep(PostSilenceBreakMs);
						}
					} else {
						_silenceStopwatch.Reset();
					}
				}

				Thread.Sleep(checkInterval);
			}
		} catch (Exception ex) {
			// Consider adding an ErrorOccurred event for notification
			Debug.WriteLine($"AudioWatcher thread error: {ex.Message}");
			OnError?.Invoke(ex);
		} finally {
			_silenceStopwatch.Reset();
		}
	}

	public void Dispose() {
		Stop();
		_thread = null;
		GC.SuppressFinalize(this);
	}

	const int PauseCheckIntervalMs = 200;
	const int PostSilenceBreakMs = 500;
}
