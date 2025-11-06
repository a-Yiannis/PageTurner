global using static PageTurner.Common;
global using static PageTurner.Ancillary.Helpers;
using System.Diagnostics;
using PageTurner.Extensions;

namespace PageTurner;

public static class Common {
	public static readonly Random RandomI = new ();
	public static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace('\\', '/');

	public static int ClicksCount { get; set; } = 0;
	public static Stopwatch Start { get; set; } = null!;
	public static string HeaderPrefix { get; set; } = $" [{DateTime.Now:HH:mm}] {nameof(PageTurner).ToTitleCase()}";
	public static string StateChar { get; set; } = "🎧";
}
