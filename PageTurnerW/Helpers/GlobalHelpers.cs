global using static  PageTurnerW.Helpers.GlobalHelpers;
using System.Diagnostics;

using System.IO;
using Common.Input;

namespace PageTurnerW.Helpers;

public static class GlobalHelpers {
	static readonly Random rnd = new ();
	public static void MoveMouseRandom(ref Mouse.POINT initial, int xMaxOffset = 10, int yMaxOffset = 5) {
		int dx = rnd.Next(-xMaxOffset, xMaxOffset + 1);
		int dy = rnd.Next(-yMaxOffset, yMaxOffset + 1);
		int newX = initial.X + dx;
		int newY = initial.Y + dy;
		Debug.WriteLine($"Moving to ({newX},{newY}) (with dx:{dx},dy:{dy}).");
		MouseAdv.MoveTo(newX, newY);
	}

	public static void Log(string message) {
		// TODO: create an actual log for Release mode.
		Debug.WriteLine($"[{DateTime.Now}] {message}");
	}

	public static void LogError(string message) {
		message = $"[{DateTime.Now}] {message}";
		File.AppendAllText(".error.log", message + Environment.NewLine);
	}
	
	public static void LogException(Exception message) {
		LogError(message.Message);
		File.AppendAllText(".stack-trace.log", $"{DateTime.Now}: {message.StackTrace}{Environment.NewLine}");
	}
}
