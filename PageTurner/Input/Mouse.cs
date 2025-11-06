using System.Runtime.InteropServices;
// ReSharper disable IdentifierTypo

namespace PageTurner.Input;

/// <summary> Provides functionality to simulate mouse input. </summary>
public static class Mouse
{
    // Imports the user32.dll function for simulating mouse events.
    [DllImport("user32.dll", SetLastError = true)]
    static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    static extern int GetSystemMetrics(int nIndex);

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct POINT {
        public readonly int X;
        public readonly int Y;
        public POINT(int x, int y) {
            X = x;
            Y = y;
        }
        public POINT((double x, double y) xy) : this((int)xy.x, (int)xy.y) { }

        public override string ToString() => $"({X}, {Y})";
        public void Deconstruct(out int x, out int y) {
            x = X;
            y = Y;
        }
    }

    // Constants for mouse events.
    const uint MOUSEEVENTF_LEFTDOWN = 0x02;
    const uint MOUSEEVENTF_LEFTUP = 0x04;
    const uint MOUSEEVENTF_MOVE = 0x0001;
    const int SM_CXSCREEN = 0;
    const int SM_CYSCREEN = 1;

    public static int GetScreenWidth() => GetSystemMetrics(SM_CXSCREEN);
    public static int GetScreenHeight() => GetSystemMetrics(SM_CYSCREEN);
    
    /// <summary>
    /// Moves the mouse cursor by the specified amount.
    /// </summary>
    /// <param name="dx">The amount to move in the X direction.</param>
    /// <param name="dy">The amount to move in the Y direction.</param>
    public static void Move(int dx, int dy) =>
        mouse_event(MOUSEEVENTF_MOVE, (uint)dx, (uint)dy, 0, UIntPtr.Zero);

    /// <summary>
    /// Sets the mouse cursor position instantly.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public static void SetMousePosition(int x, int y) => SetCursorPos(x, y);

    /// <summary> Performs a standard left mouse click. </summary>
    public static void LeftClick() => mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);

    /// <summary>
    /// Retrieves the current position of the mouse cursor.
    /// </summary>
    /// <returns>A POINT struct containing the X and Y coordinates of the cursor.</returns>
    public static POINT GetCursorPosition() {
        GetCursorPos(out var lpPoint);
        return lpPoint;
    }
}