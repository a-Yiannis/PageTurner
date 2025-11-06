namespace PageTurner.Input;


/// <summary>
/// For more keycodes:
/// https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
/// </summary>
public static class VirtualKeys {
	public const int Shift = 0x10;
	public const int Control = 0x11;
	public const int Alt = 0x12;
	// A-Z: 0x41 (A) to 0x5A (Z)
	// use: (int)ConsoleKey.A
}
