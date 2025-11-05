using System.Runtime.InteropServices;

namespace PageTurner.Input;

/// <summary> Provides advanced human-like mouse movement functionality. </summary>
public static class MouseAdv {
	[StructLayout(LayoutKind.Sequential)]
	public readonly struct POINT(int pointX, int pointY) {
		public readonly int X;
		public readonly int Y;
		public override string ToString() => $"({X}, {Y})";
	}

	// Random instance for human-like variations
	static readonly Random _random = new Random();

	// Configuration for human-like movement
	const int MIN_DURATION_MS = 200;
	const int MAX_DURATION_MS = 800;
	const double MAX_OFFSET_PERCENT = 0.1; // 10% random offset
	const int STEPS = 50; // Number of steps for smooth movement

	/// <summary>
	/// Moves the mouse cursor to the specified coordinates with natural, human-like movement.
	/// </summary>
	/// <param name="targetX">The target X coordinate.</param>
	/// <param name="targetY">The target Y coordinate.</param>
	/// <param name="durationMs">Optional duration in milliseconds for the movement.</param>
	public static void MoveMouseNatural(int targetX, int targetY, int? durationMs = null) {
		var currentPos = Mouse.GetCursorPosition();
		MoveMouseNatural(currentPos.X, currentPos.Y, targetX, targetY, durationMs);
	}

	/// <summary>
	/// Moves the mouse cursor from start to end coordinates with natural, human-like movement.
	/// </summary>
	/// <param name="startX">The starting X coordinate.</param>
	/// <param name="startY">The starting Y coordinate.</param>
	/// <param name="targetX">The target X coordinate.</param>
	/// <param name="targetY">The target Y coordinate.</param>
	/// <param name="durationMs">Optional duration in milliseconds for the movement.</param>
	public static void MoveMouseNatural(int startX, int startY, int targetX, int targetY, int? durationMs = null) {
		int actualDuration = durationMs ?? _random.Next(MIN_DURATION_MS, MAX_DURATION_MS);

		// Add small random offset to target for more human-like behavior
		double distance = Math.Sqrt(Math.Pow(targetX - startX, 2) + Math.Pow(targetY - startY, 2));
		double maxOffset = distance * MAX_OFFSET_PERCENT;

		int finalTargetX = targetX + _random.Next(-(int)maxOffset, (int)maxOffset);
		int finalTargetY = targetY + _random.Next(-(int)maxOffset, (int)maxOffset);

		// Generate control points for Bezier curve
		var controlPoints = GenerateBezierControlPoints(startX, startY, finalTargetX, finalTargetY);

		// Generate smooth path points
		var path = GenerateBezierPath(startX, startY, finalTargetX, finalTargetY,
			controlPoints.ctrlX1, controlPoints.ctrlY1,
			controlPoints.ctrlX2, controlPoints.ctrlY2, STEPS);

		// Execute the movement with variable timing
		ExecuteMovement(path, actualDuration);
	}

	/// <summary>
	/// Generates control points for a Bezier curve to create natural mouse movement.
	/// </summary>
	static (int ctrlX1, int ctrlY1, int ctrlX2, int ctrlY2) GenerateBezierControlPoints(
		int startX, int startY, int endX, int endY) {
		double distance = Math.Sqrt(Math.Pow(endX - startX, 2) + Math.Pow(endY - startY, 2));

		// Control points are offset perpendicular to the main direction for curved movement
		double angle = Math.Atan2(endY - startY, endX - startX);
		double perpendicular = angle + Math.PI / 2;

		// Randomize the curve intensity
		double curveIntensity = distance * (_random.NextDouble() * 0.3 + 0.1); // 10-40% of distance

		int ctrlX1 = startX + (int)(curveIntensity * Math.Cos(perpendicular) * (_random.NextDouble() - 0.5));
		int ctrlY1 = startY + (int)(curveIntensity * Math.Sin(perpendicular) * (_random.NextDouble() - 0.5));

		int ctrlX2 = endX + (int)(curveIntensity * Math.Cos(perpendicular) * (_random.NextDouble() - 0.5));
		int ctrlY2 = endY + (int)(curveIntensity * Math.Sin(perpendicular) * (_random.NextDouble() - 0.5));

		return (ctrlX1, ctrlY1, ctrlX2, ctrlY2);
	}

	/// <summary>
	/// Generates smooth path points using cubic Bezier curve.
	/// </summary>
	static List<POINT> GenerateBezierPath(int startX, int startY, int endX, int endY,
		int ctrlX1, int ctrlY1, int ctrlX2, int ctrlY2, int steps) {
		var path = new List<POINT>();

		for (int i = 0; i <= steps; i++) {
			double t = (double)i / steps;
			var point = CalculateBezierPoint(t, startX, startY, ctrlX1, ctrlY1, ctrlX2, ctrlY2, endX, endY);
			path.Add(new POINT((int)point.x, (int)point.y));
		}

		return path;
	}

	/// <summary>
	/// Calculates a point on a cubic Bézier curve.
	/// </summary>
	static (double x, double y) CalculateBezierPoint(double t, double x0, double y0,
			double x1, double y1, double x2, double y2,
			double x3, double y3)
	{
		double u = 1 - t;
		double tt = t * t;
		double uu = u * u;
		double uuu = uu * u;
		double ttt = tt * t;

		double x = uuu * x0 + 3 * uu * t * x1 + 3 * u * tt * x2 + ttt * x3;
		double y = uuu * y0 + 3 * uu * t * y1 + 3 * u * tt * y2 + ttt * y3;

		return (x, y);
	}

	/// <summary>
	/// Executes the movement through the path points with human-like timing variations.
	/// </summary>
	static void ExecuteMovement(List<POINT> path, int totalDurationMs) {
		int stepDelay = totalDurationMs / path.Count;

		for (int i = 0; i < path.Count; i++) {
			var point = path[i];
			Mouse.SetMousePosition(point.X, point.Y);

			// Variable delay - slower at start and end, faster in middle (ease-in/ease-out)
			double progress = (double)i / path.Count;
			double variableDelay = stepDelay * EaseInOutCubic(progress);

			// Add small random delay variation
			int actualDelay = (int)(variableDelay * (_random.NextDouble() * 0.2 + 0.9)); // ±10% variation

			Thread.Sleep(Math.Max(1, actualDelay));
		}
	}

	/// <summary>
	/// Easing function for natural acceleration/deceleration.
	/// </summary>
	static double EaseInOutCubic(double x) {
		return x < 0.5 ? 4 * x * x * x : 1 - Math.Pow(-2 * x + 2, 3) / 2;
	}
}
