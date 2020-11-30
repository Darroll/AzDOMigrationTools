using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ADO
{
    /// <summary>
    /// Class to enable proper sorting of a number passed as string.
    /// </summary>
    public class NumberAsStringComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            // Initialize.
            int result;
            Regex regex = new Regex(@"^(\d+)$");

            // x is a number written as a string.
            // y is a number written as a string.
            // Run the regex on both strings.
            Match xRegexResult = regex.Match(x);
            Match yRegexResult = regex.Match(y);

            // check if they are both numbers
            if (xRegexResult.Success && yRegexResult.Success)
            {
                // Extract
                string filenameXId = xRegexResult.Groups[1].Value;
                string filenameYId = yRegexResult.Groups[1].Value;
                int filenameXIdAsint = int.Parse(filenameXId);
                int filenameYIdAsint = int.Parse(filenameYId);

                // Compare.
                result = filenameXIdAsint.CompareTo(filenameYIdAsint);
            }
            // Use standard comparison.
            else
                result = x.CompareTo(y);

            // Return result.
            return result;
        }
    }

    /// <summary>
    /// Class to enable proper sorting of a path to a file ending with a number.
    /// This will extract parent path from path and filename and sort by parent path
    /// first and by filename second.
    /// </summary>
    public class FilenameEndsWithNumberComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            // Initialize.
            bool isValidPathX;
            bool isValidPathY = false;
            int result;
            string filenameX;
            string filenameY;
            string parentX;
            string parentY;
            Regex regex = new Regex(@"(\d+)$");

            // Validate it is a valid path for x.
            isValidPathX = Uri.TryCreate(x, UriKind.Absolute, out Uri pathUriX);
            isValidPathX = isValidPathX && pathUriX != null && pathUriX.IsLoopback;

            // Validate it is a valid path for y.
            if (isValidPathX)
            {
                isValidPathY = Uri.TryCreate(x, UriKind.Absolute, out Uri pathUriY);
                isValidPathY = isValidPathY && pathUriY != null && pathUriY.IsLoopback;
            }

            // If it has valid paths, check the filenames.
            if (isValidPathX && isValidPathY)
            {
                // x represents a path + file.
                // Extract the filename without the extension.
                parentX = System.IO.Path.GetDirectoryName(x);
                filenameX = System.IO.Path.GetFileNameWithoutExtension(x);

                // y represents a path + file.
                // Extract the filename without the extension.
                parentY = System.IO.Path.GetDirectoryName(y);
                filenameY = System.IO.Path.GetFileNameWithoutExtension(y);

                // Run the regex on both filenames.
                Match xRegexResult = regex.Match(filenameX);
                Match yRegexResult = regex.Match(filenameY);

                // Check if filename ends with a number.
                if (xRegexResult.Success && yRegexResult.Success)
                {
                    // Compare parent directory first.
                    result = parentX.CompareTo(parentY);

                    // If dealing with the same parent directories, look at the
                    // filenames.
                    if (result == 0)
                    {
                        // Extract
                        string filenameXId = xRegexResult.Groups[1].Value;
                        string filenameYId = yRegexResult.Groups[1].Value;
                        int filenameXIdAsint = int.Parse(filenameXId);
                        int filenameYIdAsint = int.Parse(filenameYId);

                        // Compare.
                        result = filenameXIdAsint.CompareTo(filenameYIdAsint);
                    }
                }
                // Use standard string comparison.
                else
                    result = x.CompareTo(y);
            }
            // Use standard string comparison.
            else
                result = x.CompareTo(y);

            // Return result.
            return result;
        }
    }

    /// <summary>
    /// Class to enable proper sorting of a path to a file starting with a number.
    /// This will extract parent path from path and filename and sort by parent path
    /// first and by filename second.
    /// </summary>
    public class FilenameStartsWithNumberComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            // Initialize.
            bool isValidPathX;
            bool isValidPathY = false;
            int result;
            string filenameX;
            string filenameY;
            string parentX;
            string parentY;
            Regex regex = new Regex(@"^(\d+)");

            // Validate it is a valid path for x.
            isValidPathX = Uri.TryCreate(x, UriKind.Absolute, out Uri pathUriX);
            isValidPathX = isValidPathX && pathUriX != null && pathUriX.IsLoopback;

            // Validate it is a valid path for y.
            if (isValidPathX)
            {
                isValidPathY = Uri.TryCreate(x, UriKind.Absolute, out Uri pathUriY);
                isValidPathY = isValidPathY && pathUriY != null && pathUriY.IsLoopback;
            }

            // If it has valid paths, check the filenames.
            if (isValidPathX && isValidPathY)
            {
                // x represents a path + file.
                // Extract the filename without the extension.
                parentX = System.IO.Path.GetDirectoryName(x);
                filenameX = System.IO.Path.GetFileNameWithoutExtension(x);

                // y represents a path + file.
                // Extract the filename without the extension.
                parentY = System.IO.Path.GetDirectoryName(y);
                filenameY = System.IO.Path.GetFileNameWithoutExtension(y);

                // Run the regex on both filenames.
                Match xRegexResult = regex.Match(filenameX);
                Match yRegexResult = regex.Match(filenameY);

                // Check if filename starts with a number.
                if (xRegexResult.Success && yRegexResult.Success)
                {
                    // Compare parent directory first.
                    result = parentX.CompareTo(parentY);

                    // If dealing with the same parent directories, look at the
                    // filenames.
                    if (result == 0)
                    {
                        // Extract
                        string filenameXId = xRegexResult.Groups[1].Value;
                        string filenameYId = yRegexResult.Groups[1].Value;
                        int filenameXIdAsint = int.Parse(filenameXId);
                        int filenameYIdAsint = int.Parse(filenameYId);

                        // Compare.
                        result = filenameXIdAsint.CompareTo(filenameYIdAsint);
                    }
                }
                // Use standard string comparison.
                else
                    result = x.CompareTo(y);
            }
            // Use standard string comparison.
            else
                result = x.CompareTo(y);

            // Return result.
            return result;
        }
    }
}