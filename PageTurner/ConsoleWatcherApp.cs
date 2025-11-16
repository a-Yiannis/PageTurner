using PageTurner.Ancillary;
using System;
using Common.Audio;
using System.Threading;

namespace PageTurner.Audio;

public static class ConsoleWatcherApp {
	public static AudioWatcher Watcher { get; set; } = new();
	
	public static void Run(Action onSilence) {
		Console.Clear();
		Console.WriteLine("🎧 PageTurner listening for system-wide silence... (Ctrl+C to stop)");

		using var progressBar = new ProgressBar(50, '-');
		var watcher = Watcher = new AudioWatcher();

		watcher.OnPeak += progressBar.Update;
		watcher.OnSilence += onSilence;

		watcher.Start();

		var exitEvent = new ManualResetEvent(false);

		Console.CancelKeyPress += (_, e) => {
			e.Cancel = true;
			watcher.Stop();
			Console.WriteLine("\n🛑 Stopped listening.");
			exitEvent.Set();
		};

		exitEvent.WaitOne();
	}
}
