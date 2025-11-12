global using static  PageTurnerW.Helpers.GlobalHelpers;
using System.Diagnostics;

namespace PageTurnerW.Helpers;

public static class GlobalHelpers {
	public static void Log(string message) {
		// TODO: create an actual log for Release mode.
		Debug.WriteLine($"[{DateTime.Now}] {message}");
	}
}
