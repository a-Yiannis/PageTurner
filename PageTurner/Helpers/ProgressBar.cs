using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace PageTurner.Ancillary;

public sealed class ProgressBar : IDisposable {
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern SafeFileHandle GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern bool WriteConsoleOutputCharacter(
        SafeFileHandle hConsoleOutput,
        string lpCharacter,
        uint nLength,
        Coord dwWriteCoord,
        out uint lpNumberOfCharsWritten);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "WriteConsoleOutputCharacterW")]
    static extern bool WriteConsoleOutputCharacter(
        SafeFileHandle hConsoleOutput,
        ref char lpCharacter, // This will accept the Span<char>
        uint nLength,
        Coord dwWriteCoord,
        out uint lpNumberOfCharsWritten);

    [StructLayout(LayoutKind.Sequential)]
    readonly struct Coord {
        public readonly short X;
        public readonly short Y;

        public Coord(short x, short y) {
            X = x;
            Y = y;
        }
    }

    const int STD_OUTPUT_HANDLE = -11;

    readonly int _barWidth;
    readonly char _fillChar;
    readonly int _decimalPlaces;
    readonly string _format;
    
    readonly SafeFileHandle _handle;
    float _lastPercentage = -1f;
    int _lastFillLength = -1;
    readonly Coord _barStartCoord;
    readonly Coord _percentageCoord;
    bool _disposed;

    public ProgressBar(int barWidth = 40, char fillChar = '=', int decimalPlaces = 1) {
        _barWidth = barWidth;
        _handle = GetStdHandle(STD_OUTPUT_HANDLE);
        _fillChar = fillChar;
        _decimalPlaces = decimalPlaces;
        _format = "p"+decimalPlaces;
        
        if (_handle.IsInvalid) 
            throw new InvalidOperationException("Failed to get stdout handle.");

        // Pre-allocate the empty bar and coordinates
        string emptyBar = new(' ', _barWidth);
        _barStartCoord = new Coord(1, (short)Console.CursorTop);
        _percentageCoord = new Coord((short)(_barWidth + 3), (short)Console.CursorTop);
        Console.CursorVisible = false;
        
        // Draw the initial empty bar
        WriteConsoleOutputCharacter(_handle, "[", 1, new Coord(0, _barStartCoord.Y), out _);
        WriteConsoleOutputCharacter(_handle, emptyBar, (uint)_barWidth, _barStartCoord, out _);
        WriteConsoleOutputCharacter(_handle, "]", 1, new Coord((short)(_barWidth + 1), _barStartCoord.Y), out _);
    }

    const int MinimumBreakPeriodMs = 500;
    int _lastUpdate = 0;
    public void Update(float percentage) {
        ObjectDisposedException.ThrowIf(_disposed, nameof(ProgressBar));

        int now = Environment.TickCount;
        if (now - _lastUpdate < MinimumBreakPeriodMs) return;
        _lastUpdate = now;

        percentage = Math.Clamp(percentage, 0f, 1f);

        // Skip if percentage hasn't meaningfully changed
        if (Math.Abs(percentage - _lastPercentage) < 0.001f) return;

        _lastPercentage = percentage;
        int fillLength = (int)(percentage * _barWidth);

        // Only update if fill length changed
        if (fillLength != _lastFillLength) {
            UpdateBarFill(fillLength);
            _lastFillLength = fillLength;
        }
        
        UpdatePercentage(percentage);
    }

    void UpdateBarFill(int fillLength) {
        // Only write the new fill characters if we're growing
        if (fillLength > _lastFillLength) {
            int startFill = Math.Max(0, _lastFillLength);
            int newFillLength = fillLength - startFill;

            if (newFillLength <= 0) return;
            
            Span<char> fillChars = stackalloc char[newFillLength];
            fillChars.Fill(_fillChar);

            Coord fillStart = new((short)(_barStartCoord.X + startFill), _barStartCoord.Y);
            WriteConsoleOutputCharacter(_handle, ref fillChars[0], (uint)newFillLength, fillStart, out _);
        }
        // If we're shrinking, clear the excess
        else if (fillLength < _lastFillLength) {
            int clearLength = _lastFillLength - fillLength;

            Span<char> clearChars = stackalloc char[clearLength];
            clearChars.Fill(' ');

            Coord clearStartCoord = new((short)(_barStartCoord.X + fillLength), _barStartCoord.Y);
            
            WriteConsoleOutputCharacter(_handle, ref clearChars[0], (uint)clearLength, clearStartCoord, out _);
        }
    }

    void UpdatePercentage(float percentage) {
        // Format percentage with padding
        byte charsCount = (byte)(7 + _decimalPlaces);
        Span<char> buffer = stackalloc char[charsCount]; // "100%  "
        if (!percentage.TryFormat(buffer, out int charsWritten, _format)) return;

        // Pad remaining space
        while (charsWritten < charsCount) buffer[charsWritten++] = ' ';
            
        // CHANGED: This now calls the 'ref char' overload, NO allocation!
        WriteConsoleOutputCharacter(_handle, ref buffer[0], charsCount, _percentageCoord, out _);
    }

    public void Complete() {
        ObjectDisposedException.ThrowIf(_disposed, nameof(ProgressBar));
        
        // Only update if not already 100%
        if (_barWidth != _lastFillLength) {
            UpdateBarFill(_barWidth);
            _lastFillLength = _barWidth;
        }
        UpdatePercentage(1f);
       
        if (!_disposed) Console.WriteLine("\nDone!");
    }

    public void Dispose() {
        if (_disposed) return;
        _handle.Dispose();
        _disposed = true;
        Console.CursorVisible = true;
    }
}