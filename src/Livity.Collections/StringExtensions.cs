using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Livity.Collections
{
	public static class StringExtensions
	{
		public static string ReIndent(this string code)
		{
			var lines = code.Split('\n').ToList();
			var firstNonBlankLine = lines.FirstOrDefault(line => line.Trim().Length > 0);
			if (firstNonBlankLine != null)
			{
				var indentation = new string(firstNonBlankLine.TakeWhile(Char.IsWhiteSpace).ToArray());
				return lines
					.Select(line => line.StartsWith(indentation) ? line.Substring(indentation.Length) : line)
					.JoinLines();
			}
			return code;
		}

		public static string JoinLines(this IEnumerable<string> enumerable)
		{
			return string.Join("\n", enumerable.ToArray());
		}

		public static string Fmt(this string format, params object[] args)
		{
			return string.Format(format, args);
		}

		public static string Join<T>(this string separator, IEnumerable<T> elements)
		{
			return elements
				.Aggregate(new StringBuilder(),
					(builder, element) => builder.Length > 0
						? builder.Append(separator).Append(element)
						: builder.Append(element))
				.ToString();
		}

		public static string WithoutSuffix(this string s, string suffix)
		{
			return s.EndsWith(suffix)
				? s.Substring(0, s.Length - suffix.Length)
				: s;
		}

		public static string UpTo(this string s, char c)
		{
			return s.Substring(0, s.IndexOf(c));
		}

		public static string WithoutPrefix(this string s, string prefix)
		{
			return s.StartsWith(prefix)
				? s.Substring(prefix.Length, s.Length - prefix.Length)
				: s;
		}

		public static string WithPrefix(this string s, string prefix)
		{
			return prefix + s;
		}

		public static string NormalizeLineTerminators(this string text)
		{
			return text.Replace("\r\n", "\n");
		}
	}
}