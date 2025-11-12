using PageTurner.Ancillary;
using System;
using Common.Audio;

namespace PageTurner.Audio;

public static class ConsoleWatcherApp {
	public static AudioWatcher Watcher { get; set; } = new();
	
	public static void Run(Action onSilence) {
		Console.Clear();
		Console.WriteLine("🎧 PageTurner listening for system-wide silence... (Ctrl+C to stop)");

		using var progressBar = new ProgressBar(50, '-');
		var watcher = Watcher = new AudioWatcher();

		watcher.PeakMeasured += progressBar.Update;
		watcher.SilenceDetected += onSilence;

		watcher.Start();

		Console.CancelKeyPress += (_, e) => {
			e.Cancel = true;
			watcher.Stop();
			Console.WriteLine("\n🛑 Stopped listening.");
		};

		// Keep main thread alive
		while (watcher.IsRunning)
			Thread.Sleep(500);
	}
}
