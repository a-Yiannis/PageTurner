using System.Runtime.CompilerServices;
using System.Text;

namespace PageTurner.Extensions;

public static class StringEx {
	public static string ToTitleCase(this string? s) {
		if (s is null) return "";
		int len = s.Length;
		if (len == 0) return s;

		var src = s.AsSpan();

		// Count spaces using "smart" boundaries:
		// - lower -> UPPER
		// - digit -> letter
		// - UPPER -> UPPER followed by lower (acronym boundary)
		int spaces = 0;
		for (int i = 1; i < len; i++) {
			if (IsBoundary(src, i)) spaces++;
		}

		if (spaces == 0) return s;
		return string.Create(len + spaces, s, static (dst, srcStr) => {
			var sp = srcStr.AsSpan();
			int w = 0;

			dst[w++] = sp[0];

			for (int i = 1; i < sp.Length; i++)
			{
				if (IsBoundary(sp, i))
					dst[w++] = ' ';
				dst[w++] = sp[i];
			}
		});

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static bool IsBoundary(ReadOnlySpan<char> span, int i) {
			char prev = span[i - 1];
			char curr = span[i];

			if (char.IsLower(prev) && char.IsUpper(curr)) return true;
			if (char.IsDigit(prev) && char.IsLetter(curr)) return true;

			if (char.IsUpper(prev) && char.IsUpper(curr))
				return i + 1 < span.Length && char.IsLower(span[i + 1]);

			return false;
		}
	}
}
