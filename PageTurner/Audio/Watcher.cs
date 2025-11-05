namespace PageTurner.Audio;

public class Watcher {
	const float Threshold = 0.05f; // Audio level below which is considered silence.
	const int CheckInterval = 500; // Milliseconds between audio level checks.
	const int PauseMs = 5000; // Milliseconds of silence required to trigger a click.

	public static void StartWatching(Action callback) {
		Console.WriteLine("🎧 PageTurner listening for system-wide silence... (Ctrl+C to stop)");

		int silentTime = 0;

		while (true) {
			// Get the peak audio value from the system audio.
			float peak = WASAPI.GetSystemPeakValue();
			Console.WriteLine($"peak: {peak:F4}"); // Display with 4 decimal places for clarity

			if (peak < Threshold) {
				// If audio is below the threshold, increment the silent time.
				silentTime += CheckInterval;
				if (silentTime >= PauseMs) {
					// If silent for long enough, perform a click and reset.
					callback();
					silentTime = 0;
					Thread.Sleep(500); // Brief pause after clicking.
				}
			} else {
				// If audio is detected, reset the silent time.
				silentTime = 0;
			}

			Thread.Sleep(CheckInterval);
		}
	}
}
