namespace Common.Audio;

/// <summary>
/// Continuously monitors system audio and triggers a callback after a period of silence.
/// </summary>
public class AudioWatcher {
	readonly float _threshold;
	readonly int _checkIntervalMs;
	readonly int _silenceDurationMs;
	Thread? _thread;
	volatile bool _isRunning;

	public bool IsPaused { get; set; }
	public bool IsRunning => _isRunning;

	/// <summary> Raised with the current peak level on each check. </summary>
	public event Action<float>? PeakMeasured;

	/// <summary> Raised when the required silence duration is detected. </summary>
	public event Action? SilenceDetected;

	public AudioWatcher(int checkIntervalMs = 200, float threshold = 0.05f, int silenceDurationMs = 2000) {
		_threshold = threshold;
		_checkIntervalMs = checkIntervalMs;
		_silenceDurationMs = silenceDurationMs;
	}

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
		int silentTime = 0;

		while (_isRunning) {
			if (IsPaused) {
				Thread.Sleep(100);
				continue;
			}

			float peak = WASAPI.GetSystemPeakValue();
			PeakMeasured?.Invoke(peak);

			if (peak < _threshold) {
				silentTime += _checkIntervalMs;
				if (silentTime >= _silenceDurationMs) {
					SilenceDetected?.Invoke();
					silentTime = 0;
					Thread.Sleep(5000); // Post-silence cooldown
				}
			} else {
				silentTime = 0;
			}

			Thread.Sleep(_checkIntervalMs);
		}
	}
}
