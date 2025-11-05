using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace PageTurner;

public class ProgressBar : IDisposable {
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern SafeFileHandle GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool WriteConsoleOutputCharacter(
        SafeFileHandle hConsoleOutput,
        string lpCharacter,
        uint nLength,
        Coord dwWriteCoord,
        out uint lpNumberOfCharsWritten);

    [StructLayout(LayoutKind.Sequential)]
    struct Coord {
        public short X;
        public short Y;

        public Coord(short x, short y) {
            X = x;
            Y = y;
        }
    }

    const int STD_OUTPUT_HANDLE = -11;

    readonly int _barWidth;
    readonly char _fillChar;
    readonly string _format;
    
    readonly SafeFileHandle _handle;
    float _lastPercentage = -1f;
    int _lastFillLength = -1;
    readonly Coord _barStartCoord;
    readonly Coord _percentageCoord;
    bool _disposed = false;

    public ProgressBar(int barWidth = 40, char fillChar = '=', string format = "F1") {
        _barWidth = barWidth;
        _handle = GetStdHandle(STD_OUTPUT_HANDLE);
        _fillChar = fillChar;
        _format = format;
        
        if (_handle.IsInvalid) 
            throw new InvalidOperationException("Failed to get stdout handle.");

        // Pre-allocate the empty bar and coordinates
        string emptyBar = new(' ', _barWidth);
        _barStartCoord = new Coord(1, (short)Console.CursorTop);
        _percentageCoord = new Coord((short)(_barWidth + 3), (short)Console.CursorTop);
        
        // Draw initial empty bar
        WriteConsoleOutputCharacter(_handle, "[", 1, new Coord(0, _barStartCoord.Y), out _);
        WriteConsoleOutputCharacter(_handle, emptyBar, (uint)_barWidth, _barStartCoord, out _);
        WriteConsoleOutputCharacter(_handle, "]", 1, new Coord((short)(_barWidth + 1), _barStartCoord.Y), out _);
    }

    public void Update(float percentage) {
        if (_disposed) throw new ObjectDisposedException(nameof(ProgressBar));
        
        percentage = Math.Clamp(percentage, 0f, 100f);

        // Skip if percentage hasn't meaningfully changed
        if (Math.Abs(percentage - _lastPercentage) < 0.1f) return;

        _lastPercentage = percentage;
        int fillLength = (int)(percentage * _barWidth / 100f);

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
            
            if (newFillLength > 0) {
                string fillChars = new(_fillChar, newFillLength);
                Coord fillStart = new((short)(_barStartCoord.X + startFill), _barStartCoord.Y);
                WriteConsoleOutputCharacter(_handle, fillChars, (uint)newFillLength, fillStart, out _);
            }
        }
        // If we're shrinking (unusual for progress bars), clear the excess
        else if (fillLength < _lastFillLength) {
            int clearStart = fillLength;
            int clearLength = _lastFillLength - fillLength;
            string clearChars = new(' ', clearLength);
            Coord clearStartCoord = new((short)(_barStartCoord.X + clearStart), _barStartCoord.Y);
            WriteConsoleOutputCharacter(_handle, clearChars, (uint)clearLength, clearStartCoord, out _);
        }
    }

    void UpdatePercentage(float percentage) {
        // Format percentage with padding to overwrite previous value
        // Using stack allocation for small strings to avoid heap allocation
        Span<char> buffer = stackalloc char[8]; // "100.0%  "
        if (percentage.TryFormat(buffer, out int charsWritten, _format)) {
            buffer[charsWritten++] = '%';
            // Pad remaining space with spaces
            while (charsWritten < 7) buffer[charsWritten++] = ' ';
            
            WriteConsoleOutputCharacter(_handle, new string(buffer[..7]), 7, _percentageCoord, out _);
        }
    }

    public void Complete() {
        Update(100f);
        if (!_disposed) {
            Console.WriteLine("\nDone!");
        }
    }

    public void Dispose() {
        if (!_disposed) {
            _handle?.Dispose();
            _disposed = true;
        }
    }
}