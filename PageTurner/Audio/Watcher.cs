using PageTurner.Ancillary;

namespace PageTurner.Audio;

public static class Watcher {
	const float Threshold = 0.05f;      // Audio level below which is considered silence.
	const int CheckIntervalMs = 500;    // Milliseconds between audio level checks.
	const int SilenceDurationMs = 2000; // Milliseconds of silence required to trigger a click.
	
	// TODO: create a pause mechanism
	public static bool IsPaused { get; set; } = false;

	public static void Watch(Action callback) {
		Console.Clear();

		Console.WriteLine("🎧 PageTurner listening for system-wide silence... (Ctrl+C to stop)");

		int silentTime = 0;
		using var progressBar = new ProgressBar(50, '=');
		while (true) {
			if (IsPaused) {
				Thread.Sleep(100);
				continue;
			}
			// Get the peak audio value from the system audio.
			float peak = WASAPI.GetSystemPeakValue();
			progressBar.Update(peak);
			// Console.WriteLine($"peak: {peak:F4}"); // Display with 4 decimal places for clarity

			if (peak < Threshold) {
				// If audio is below the threshold, increase the silent time.
				silentTime += CheckIntervalMs;
				if (silentTime >= SilenceDurationMs) {
					// If silent for long enough, perform a click and reset.
					callback();
					silentTime = 0;
					Thread.Sleep(5000); // Brief pause after clicking.
				}
			} else {
				// If audio is detected, reset the silent time.
				silentTime = 0;
			}

			Thread.Sleep(CheckIntervalMs);
		}
	}
}
