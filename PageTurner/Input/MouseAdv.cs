using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace PageTurner.Input;

/// <summary>
/// Provides advanced human-like mouse movement functionality.
/// </summary>
public static class MouseAdv {
	static readonly Random _random = new();

	const int MIN_DURATION_MS = 200;
	const int MAX_DURATION_MS = 800;
	const double MAX_OFFSET_PERCENT = 0.1; // 10% random offset
	const int MIN_STEPS = 50;
	const int MAX_STEPS = 100;
	const double DISTANCE_PER_STEP = 30; // pixels per step scaling

	/// <summary>
	/// Moves the mouse cursor to the specified coordinates with natural, human-like movement.
	/// </summary>
	public static void MoveTo(int targetX, int targetY, int? durationMs = null, CancellationToken? token = null) {
		var currentPos = Mouse.GetCursorPosition();
		MoveTo(currentPos.X, currentPos.Y, targetX, targetY, durationMs, token);
	}

	/// <summary>
	/// Moves the mouse cursor from start to end coordinates with natural, human-like movement.
	/// </summary>
	public static void MoveTo(int startX, int startY, int targetX, int targetY, int? durationMs = null, CancellationToken? token = null) {
		int actualDuration = durationMs ?? _random.Next(MIN_DURATION_MS, MAX_DURATION_MS);

		// Calculate distance
		double distance = Math.Sqrt(Math.Pow(targetX - startX, 2) + Math.Pow(targetY - startY, 2));

		// Add a small random offset to the target
		double maxOffset = distance * MAX_OFFSET_PERCENT;
		int screenWidth = Mouse.GetScreenWidth();
		int screenHeight = Mouse.GetScreenHeight();

		int finalTargetX = Math.Clamp(targetX + _random.Next(-(int)maxOffset, (int)maxOffset + 1), 0, screenWidth - 1);
		int finalTargetY = Math.Clamp(targetY + _random.Next(-(int)maxOffset, (int)maxOffset + 1), 0, screenHeight - 1);

		// Generate curve control points
		var controlPoints = GenerateBezierControlPoints(startX, startY, finalTargetX, finalTargetY);

		// Adaptive step count based on distance
		int steps = Math.Clamp((int)(distance / DISTANCE_PER_STEP), MIN_STEPS, MAX_STEPS);

		// Generate a smooth Bezier path (floating-point precision)
		var path = GenerateBezierPath(startX, startY, finalTargetX, finalTargetY,
			controlPoints.ctrlX1, controlPoints.ctrlY1,
			controlPoints.ctrlX2, controlPoints.ctrlY2, steps);

		// Execute smooth motion
		ExecuteMovement(path, actualDuration, token);
	}

	static (int ctrlX1, int ctrlY1, int ctrlX2, int ctrlY2) GenerateBezierControlPoints(int startX, int startY, int endX, int endY) {
		double distance = Math.Sqrt(Math.Pow(endX - startX, 2) + Math.Pow(endY - startY, 2));
		double angle = Math.Atan2(endY - startY, endX - startX);
		double perpendicular = angle + Math.PI / 2;
		double curveIntensity = distance * (_random.NextDouble() * 0.3 + 0.1); // 10–40% of distance

		int ctrlX1 = startX + (int)(curveIntensity * Math.Cos(perpendicular) * (_random.NextDouble() - 0.5));
		int ctrlY1 = startY + (int)(curveIntensity * Math.Sin(perpendicular) * (_random.NextDouble() - 0.5));
		int ctrlX2 = endX + (int)(curveIntensity * Math.Cos(perpendicular) * (_random.NextDouble() - 0.5));
		int ctrlY2 = endY + (int)(curveIntensity * Math.Sin(perpendicular) * (_random.NextDouble() - 0.5));

		return (ctrlX1, ctrlY1, ctrlX2, ctrlY2);
	}

	static List<(double x, double y)> GenerateBezierPath(int startX, int startY, int endX, int endY,
		int ctrlX1, int ctrlY1, int ctrlX2, int ctrlY2, int steps)
	{
		var path = new List<(double x, double y)>(steps + 1);
		for (int i = 0; i <= steps; i++) {
			double t = (double)i / steps;
			path.Add(CalculateBezierPoint(t, startX, startY, ctrlX1, ctrlY1, ctrlX2, ctrlY2, endX, endY));
		}
		return path;
	}

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
	/// Executes the movement through the path points with human-like timing and exact total duration.
	/// </summary>
	static void ExecuteMovement(List<(double x, double y)> path, int totalDurationMs, CancellationToken? token = null) {
		// Precompute easing values and normalize timing
		int n = path.Count;
		double[] ease = new double[n];
		double totalEase = 0;

		for (int i = 0; i < n; i++) {
			ease[i] = EaseInOutCubic((double)i / (n - 1));
			totalEase += ease[i];
		}
		for (int i = 0; i < n; i++) ease[i] /= totalEase; // Normalize

		var sw = Stopwatch.StartNew();
		double elapsed = 0;

		for (int i = 0; i < n; i++) {
			if (token?.IsCancellationRequested == true)
				break;

			var (x, y) = path[i];
			Mouse.SetMousePosition((int)Math.Round(x), (int)Math.Round(y));

			double stepTarget = elapsed + ease[i] * totalDurationMs;
			while (sw.ElapsedMilliseconds < stepTarget) Thread.SpinWait(50);

			elapsed = stepTarget;
		}

		// Optional micro-jitter at end
		if (_random.NextDouble() < 0.4) {
			var end = path[^1];
			int jitterX = (int)Math.Round(end.x) + _random.Next(-1, 2);
			int jitterY = (int)Math.Round(end.y) + _random.Next(-1, 2);
			Mouse.SetMousePosition(jitterX, jitterY);
		}
	}

	static double EaseInOutCubic(double x)
		=> x < 0.5 ? 4 * x * x * x : 1 - Math.Pow(-2 * x + 2, 3) / 2;
}
