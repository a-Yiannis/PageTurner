using System.Text;
using PageTurner;
using PageTurner.Audio;
using PageTurner.Input;
using static PageTurner.Input.VirtualKeys;

Console.OutputEncoding = Encoding.UTF8;

var initialPosition = Mouse.GetCursorPosition();
for (int j = 0; j < 5; j++) {
	MoveRandom(ref initialPosition);
	Thread.Sleep(500);
}

return;

HotkeyListener listener = new ();
listener.Register(OnCtrlAltNK, Control, Alt, K);
listener.Start();
Console.WriteLine("Waiting for Ctrl+Alt+K to start the Lister+Click loop.");
Console.WriteLine("Press 'q', in console to quit.");
while (Console.ReadKey(true).KeyChar != 'q');
Console.WriteLine("Q has been pressed. Goodbye.");

void OnCtrlAltNK() {
	listener.Stop();
	listener.Dispose();

	var initialPosition = Mouse.GetCursorPosition();
	var click = Click;
	Watcher.StartWatching(click);
	
	void Click() {
		// add a little chaos to simulate reality even more
		Thread.Sleep(Rnd.Next(100, 250));
		MoveRandom(ref initialPosition);
	}
}