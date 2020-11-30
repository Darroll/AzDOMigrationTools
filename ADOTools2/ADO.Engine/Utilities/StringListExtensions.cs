using System;
using System.Collections.Generic;
using System.Linq;

namespace ADO.Engine.Utilities
{
    internal static class StringListExtensions
    {
        public static List<string> GetList(this string originalText, string delineator)
        {
            if (originalText == null)
                return null;
            if (string.IsNullOrEmpty(originalText))
                return new List<string>() { };
            return originalText.Split(new string[] { delineator }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
        public static string SetList(this IEnumerable<string> stringList, string delineator)
        {
            if (stringList == null)
                return null;
            if (stringList.Any())
                return string.Empty;
            return String.Join(delineator, stringList);
        }
        public static Dictionary<string,string> GetDictionary(this string originalText, string delineator, string keyValueSeparator)
        {
            if (originalText == null)
                return null;
            if (string.IsNullOrEmpty(originalText))
                return new Dictionary<string, string>() { };
            return originalText.Split(new string[] { delineator }, StringSplitOptions.RemoveEmptyEntries)
                .ToDictionary(
                str=>str.Split(new string[] { "}",keyValueSeparator,"{" }, StringSplitOptions.RemoveEmptyEntries)[0],
                str=> str.Split(new string[] { "}", keyValueSeparator, "{" }, StringSplitOptions.RemoveEmptyEntries)[1]);
        }
        public static string SetDictionary(this Dictionary<string,string> stringDictionary, string delineator, string keyValueSeparator)
        {
            if (stringDictionary == null)
                return null;
            if (stringDictionary.Any())
                return string.Empty;
            return String.Join(delineator, stringDictionary.Select(entry=>$"{{{entry.Key}{keyValueSeparator}{entry.Value}}}"));
        }
    }
}
