using System.Diagnostics;
using System.Text;
using PageTurner.Ancillary;
using PageTurner.Audio;
using PageTurner.Extensions;
using PageTurner.Input;
using static PageTurner.Ancillary.ConsoleHelpers;
using static PageTurner.Input.VirtualKeys;

const ConsoleKey escapeKey = ConsoleKey.Q;
const ConsoleKey starterKey = ConsoleKey.W;

Start = Stopwatch.StartNew();

Console.OutputEncoding = Encoding.UTF8;
Console.Title = HeaderPrefix;
Console.WindowWidth = 68;

using var headerTimer = new Timer(_ => UpdateHeader(), null, 0, 1000);
SetPaused();
HotkeyListener listener = new ();

listener.Register(StartLoop, Control, Alt, (int)ConsoleKey.W);
listener.Start();
WriteColored($"Press |c|^!{starterKey}| to start the page turner.\n");
WriteColored($"Press '|c|{escapeKey}|' to quit.");
// Thread.Sleep(3000);
// StartLoop();
while (Console.ReadKey(true).Key != escapeKey);
Console.WriteLine("|c|Q| has been pressed.\nGoodbye!👋");
return;

void StartLoop() {
	listener.Stop();
	listener.Unregister(StartLoop);
	TogglePause();

	listener.Register(TogglePause, Control, Alt, (int)ConsoleKey.W);
	listener.Start();

	var initialPosition = Mouse.GetCursorPosition();
	Watcher.Watch(Click);
	
	void Click() {
		// add a little chaos to simulate reality even more
		MoveMouseRandom(ref initialPosition);
		Thread.Sleep(RandomI.Next(100, 250));
		MoveMouseRandom(ref initialPosition);
		Mouse.LeftClick();
		ClicksCount++;
	}
}
