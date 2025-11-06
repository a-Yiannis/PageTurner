using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace PageTurner.Ancillary;

public static partial class ConsoleHelpers {
	#region Utilities
	static void insertTime(ref string message, [CallerMemberName] string caller = "")
		=> message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}]: [{caller}] {message}";
	#endregion

	#region Error
	[DoesNotReturn]
	public static void FatalError(Exception e, int exitCode) => FatalError(e.ToString(), exitCode);
	[DoesNotReturn]
	public static void FatalError(string msg, ExitCode exitCode) => FatalError(msg, (int)exitCode);

	// Keep track of the exit code so they can be easily communicated
	public enum ExitCode {
		Unknown = -1,
		Success = 0
	}

	[DoesNotReturn]
	public static void FatalError(string message, int exitCode) {
		Error(message);
		Console.WriteLine();
		Console.WriteLine("Goodbye!");
		Environment.Exit(exitCode); // Attempt graceful termination
		// Fallback to FailFast if Exit fails
		Environment.FailFast($"Fatal error occurred with exit code {exitCode}: {message}");
		throw new ApplicationException("Unreachable code! It should never happen.");
	}

	/// <summary> Writes an error message with red color and beeps. </summary>
	public static void Error(string message) {
		insertTime(ref message);
		try {
			File.AppendAllText(".error.log", message);
		} catch {
			/* ignored */
		}
		// make sure that it starts on the start of the line.
		if (Console.CursorLeft != 0) Console.WriteLine();
		WriteLineWithColor(message, ConsoleColor.Red);
		// make a sound to draw users attention
		Console.Beep();

		Console.Write("Press any key to continue...");
		Console.ReadLine();
	}
	#endregion

	/// <summary> Writes the message with a [WARN] diacritic and a yellow color. </summary>
	public static void Warn(string message) {
		insertTime(ref message);
		// make sure that it starts on the start of the line.
		if (Console.CursorLeft != 0) Console.WriteLine();
		WriteLineWithColor(message, ConsoleColor.DarkYellow);
	}

	/// <summary> Writes a new line with the given color and restores the original color. </summary>
	public static void WriteLineWithColor(string message, ConsoleColor color) {
		var oColor = Console.ForegroundColor;
		if (oColor == Console.ForegroundColor) {
			Console.WriteLine(message);
			return;
		}

		Console.ForegroundColor = color;
		// make sure that it starts on the start of the line.
		if (Console.CursorLeft != 0) Console.WriteLine();
		Console.WriteLine(message);
		// restore original color
		Console.ForegroundColor = oColor;
	}

	/// <summary> Inserts the current time at the start of the message and logs the message into the main.log. </summary>
	public static void Log(string message) {
		insertTime(ref message);
		Console.WriteLine(message);
	}

	static ConsoleColor emphaticColor = ConsoleColor.DarkYellow;
	public static ConsoleColor EmphaticColor { get => emphaticColor; set => emphaticColor = value; }
	public static void WriteEmphatic(string input, params object?[] parts) {
		if (parts.Length == 0) {
			Console.Write(input);
			return;
		}

		var oColor = Console.ForegroundColor;
		Console.Write(input);
		bool emphatic = false;

		foreach (object? part in parts) {
			switch (part) {
				case string s:
					writePart(s);
					break;
				case IEnumerable<char> chars:
					writePart(string.Join("", chars));
					break;
				case IEnumerable enumerable:
					foreach (object? p in enumerable)
						writePart(p?.ToString());
					break;
				default:
					writePart(part?.ToString());
					break;
			}
		}
		Console.ForegroundColor = oColor;
		return;

		void writePart(string? part) {
			emphatic = !emphatic;
			Console.ForegroundColor = emphatic ? emphaticColor : oColor;
			Console.Write(part ?? "null");
		}
	}

	public static void WriteColored(string input) {
		var originalColor = Console.ForegroundColor;
		int lastIndex = 0;
		var matches = tokenRegex().Matches(input);

		foreach (Match match in matches) {
			// Write text before the match
			if (match.Index > lastIndex) {
				Console.ForegroundColor = originalColor;
				Console.Write(input[lastIndex..match.Index]);
			}

			// Parse color (default to DarkCyan)
			string colorValue = match.Groups["color"].Value;
			if (!Enum.TryParse(colorValue, true, out ConsoleColor color))
				color = ConsoleColor.DarkCyan;

			// Write the colored word
			Console.ForegroundColor = color;
			Console.Write(match.Groups["word"].Value);
			Console.ForegroundColor = originalColor;

			lastIndex = match.Index + match.Length;
		}

		// Write remaining text
		if (lastIndex >= input.Length) return;
		Console.ForegroundColor = originalColor;
		Console.Write(input[lastIndex..]);
	}
	public static int Pick(string initialQuery, params string[] options) {
		if (options.Length == 0)
			throw new ArgumentException("At least one option is required");

		int selectedIndex = 0;

		// Save starting position instead of clearing
		int startLeft = Console.CursorLeft;
		int startTop = Console.CursorTop;

		WriteColored(initialQuery);
		Console.WriteLine();

		// Store marker positions (exact cursor coords)
		(int left, int top)[] markerPos = new (int, int)[options.Length];

		for (int i = 0; i < options.Length; i++) {
			// Remember where marker will be drawn
			int left = Console.CursorLeft;
			int top = Console.CursorTop;
			markerPos[i] = (left, top);

			// Reserve 2 spaces for the marker, then draw the option text in color
			Console.Write("  ");
			WriteColored($"{i + 1}. {options[i]}");
			Console.WriteLine();
		}

		Console.WriteLine("\n[↑/↓] Navigate | [1-9] Jump | [Enter] Select");

		void SetMarker(int index, char marker) {
			(int l, int t) = markerPos[index];
			try {
				Console.SetCursorPosition(l, t);
				Console.Write(marker);
			} catch (IOException) {
				// console resized or invalid position
			}
		}

		// Draw initial marker
		SetMarker(selectedIndex, '>');

		while (true) {
			var key = Console.ReadKey(intercept: true);
			int oldIndex = selectedIndex;

			switch (key.Key) {
				case ConsoleKey.Escape:
					Console.SetCursorPosition(startLeft, markerPos[^1].top + 3);
					WriteColored(nameof(Pick) + " |c=Red|aborted| by |c|ESCAPE|.");
					Console.WriteLine();
					return -1;

				case ConsoleKey.UpArrow:
					selectedIndex = (selectedIndex - 1 + options.Length) % options.Length;
					break;

				case ConsoleKey.DownArrow:
					selectedIndex = (selectedIndex + 1) % options.Length;
					break;

				case ConsoleKey.Enter:
					// Move the cursor below the menu
					Console.SetCursorPosition(startLeft, markerPos[^1].top + 3);
					Console.WriteLine($"{nameof(Pick)}ed: |c|{selectedIndex}|");
					WriteColored(options[selectedIndex]);
					return selectedIndex;

				case >= ConsoleKey.D1 and <= ConsoleKey.D9:
					int choice = key.KeyChar - '0' - 1;
					if (choice < options.Length)
						selectedIndex = choice;
					else
						Console.Beep();
					break;

				default:
					Console.Beep();
					break;
			}

			if (oldIndex == selectedIndex) continue;
			SetMarker(oldIndex, ' ');
			SetMarker(selectedIndex, '>');
		}
	}

	public static string ConsoleLink(string link) => ConsoleLink(link[^20..], link);
	public static string ConsoleLink(string displayText, string link) =>
		$"\e]8;;{link}\a{displayText}\e]8;;\a";

	[GeneratedRegex(@"\|c(?:=(?<color>[^|]+?))?\| *?(?<word>.+?)\|", RegexOptions.IgnoreCase)]
	private static partial Regex tokenRegex();
}
