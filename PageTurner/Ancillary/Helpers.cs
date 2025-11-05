using PageTurner.Input;

namespace PageTurner.Ancillary;

public static class Helpers {
	static readonly Random rnd = new ();
	public static void MoveRandom(ref Mouse.POINT initial, int xMaxOffset = 40, int yMaxOffset = 20) {
		int xOffset = rnd.Next(0, 2 * xMaxOffset + 1) - xMaxOffset;
		int yOffset = rnd.Next(0, 2 * yMaxOffset + 1) - yMaxOffset;
	
		int newX = initial.X + xOffset;
		int newY = initial.Y + yOffset;
		Console.WriteLine($"Moving to {newX}, {newY}.");
		Mouse.MoveMouse(
			xOffset,
			yOffset);
	}
}
