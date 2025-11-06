using System.Runtime.InteropServices;

namespace PageTurner.Input;

public partial class HotkeyListener  {
	[LibraryImport("user32.dll")]
	public static partial short GetAsyncKeyState(int vKey);

	Thread? _listenerThread;
	volatile bool _isRunning;
	readonly List<HotkeyBinding> _bindings;
	readonly int _pollInterval;

	public HotkeyListener(int pollInterval = 150) {
		_pollInterval = Math.Max(5, pollInterval);
		_bindings = [ ];
	}


	public void Register(Action callback, params int[] keys) {
		if (keys == null || keys.Length == 0) throw new ArgumentException("Keys required", nameof(keys));

		_bindings.Add(new HotkeyBinding(callback, keys, TimeSpan.FromMilliseconds(500)));
	}

	// Unregister by callback reference (works for methods; for lambdas, keep reference)
	public bool Unregister(Action callback)
		=> _bindings.RemoveAll(b => b.Callback == callback) > 0;

	public void ClearAll() => _bindings.Clear();

	public void Start() {
		if (_isRunning) return;

		_isRunning = true;
		_listenerThread = new Thread(Listen) {
			IsBackground = true,
			Name = nameof(HotkeyListener)
		};
		_listenerThread.Start();
	}

	public void Stop() {
		_isRunning = false;
		_listenerThread?.Join(1000);
		_listenerThread = null;
	}

	void Listen() {
		while (_isRunning) {
			foreach (var binding in _bindings) {
				if (!IsKeyComboPressed(binding.Keys, exact: true)) continue;

				var now = DateTime.UtcNow;
				if (binding.LastTriggered.HasValue && now - binding.LastTriggered.Value <= binding.DebounceInterval)
					continue; // Debounce: Skip if too recent

				binding.LastTriggered = now;
				try {
					binding.Callback?.Invoke();
				} catch (Exception ex) {
					// Optional: Log or handle exceptions from user callbacks
					Console.Error.WriteLine($"Hotkey callback error: {ex.Message}");
				}
			}

			Thread.Sleep(_pollInterval);
		}
	}

	// exact: true requires only these keys down (ignores others); false requires all these AND no others (stricter, rarely useful)
	static bool IsKeyComboPressed(int[] keys, bool exact = true) {
		if (exact) {
			foreach (int key in keys) {
				if ((GetAsyncKeyState(key) & 0x8000) == 0) return false;
			}
			return true;
		}
		// All required down (skip "no extras" for simplicity)
		foreach (int key in keys) {
			if ((GetAsyncKeyState(key) & 0x8000) == 0) return false;
		}
		return true;
	}
}

internal class HotkeyBinding {
	public Action Callback { get; }
	public int[] Keys { get; }
	public TimeSpan DebounceInterval { get; }
	public DateTime? LastTriggered { get; set; }

	public HotkeyBinding(Action callback, int[] keys, TimeSpan debounceInterval) {
		Callback = callback ?? throw new ArgumentNullException(nameof(callback));
		Keys = keys ?? throw new ArgumentNullException(nameof(keys));
		DebounceInterval = debounceInterval;
	}
}
