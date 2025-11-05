global using static PageTurner.Common;
global using static PageTurner.Ancillary.Helpers;

namespace PageTurner;

public static class Common {
	public static Random Rnd = new ();
	public static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace('\\', '/');
}
