using System.Diagnostics;
using Common.Input;
using Common;
using PageTurner.Audio;

namespace PageTurner.Helpers;

public static class Helpers {
	static readonly Random rnd = new ();
	public static void MoveMouseRandom(ref Mouse.POINT initial, int xMaxOffset = 10, int yMaxOffset = 5) {
		int dx = rnd.Next(-xMaxOffset, xMaxOffset + 1);
		int dy = rnd.Next(-yMaxOffset, yMaxOffset + 1);
		int newX = initial.X + dx;
		int newY = initial.Y + dy;
		Debug.WriteLine($"Moving to ({newX},{newY}) (with dx:{dx},dy:{dy}).");
		MouseAdv.MoveTo(newX, newY);
	}

	public static void UpdateHeader() => Console.Title = $@"{StateChar} {HeaderPrefix} ({ClicksCount}, {Start.Elapsed:hh\:mm\:ss})";
}
