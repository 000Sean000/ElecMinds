using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace PKG
{
	public static class ParsingPKG
	{
		public static string? GetMatchingSubstring(string input, Regex pattern)
		{
			Match match = pattern.Match(input);
			if (match.Success)
			{
				return match.Value;
			}
			else { return null; }
		}
		public static List<string> GetMatchingSubstrings(string input, Regex pattern)
		{
			List<string> matches = new List<string>();
			MatchCollection matchCollection = pattern.Matches(input);

			foreach (Match match in matchCollection)
			{
				matches.Add(match.Value);
			}

			return matches;
		}
		public static List<string> GetNonMatchingSubstrings(string input, Regex pattern)
		{
			List<string> nonMatchingSubstrings = new List<string>();
			string[] substrings = pattern.Split(input);

			foreach (string substring in substrings)
			{
				if (!pattern.IsMatch(substring))
				{
					nonMatchingSubstrings.Add(substring);
				}
			}

			return nonMatchingSubstrings;
		}

		public static List<string> ParseWithPattern(string input, Regex pattern)
		{
			List<string> substrings = new List<string>();

			// Modify the pattern to capture the matching delimiters
			string modifiedPattern = $"({pattern})";

			// Split the input string using the modified pattern
			string[] splitResult = Regex.Split(input, modifiedPattern);

			// Add the substrings to the list, including the pattern itself
			foreach (string substr in splitResult)
			{
				substrings.Add(substr);
			}

			return substrings;
		}


	}
}
