using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ADO.Engine.Tests
{
    [TestClass]
    public sealed class CustomComparersTest
    {
        [TestMethod]
        [Ignore()] //fix this one day
        public void TestSortWithFilenames()
        {
            FilenameEndsWithNumberComparer customComparer = new FilenameEndsWithNumberComparer();
            string[] files = new string[]
            {
                "file1",
                "file10",
                "file11",
                "file12",
                "file2",
                "file3",
                "file4",
                "file5",
                "file6",
                "file7",
                "file8",
                "file9"
            };

            // Sort files to make sure they are processed in the right order.
            Array.Sort(files, customComparer);

            // This list should be in order.
            CollectionAssert.AreEqual(new[] { "file1", "file2", "file3", "file4", "file5", "file6", "file7", "file8", "file9", "file10", "file11", "file12" }, files);
        }

        [TestMethod]
        public void TestSortWithPaths()
        {
            FilenameEndsWithNumberComparer customComparer = new FilenameEndsWithNumberComparer();
            string[] files = new string[]
            {
                @"C:\myPath\a\b\c\file1",
                @"C:\myPath\a\b\c\file10",
                @"C:\myPath\a\b\c\file11",
                @"C:\myPath\a\b\c\file12",
                @"C:\myPath\a\b\c\file2",
                @"C:\myPath\a\b\c\file3",
                @"C:\myPath\a\b\c\file4",
                @"C:\myPath\a\b\c\file5",
                @"C:\myPath\a\b\c\file6",
                @"C:\myPath\a\b\c\file7",
                @"C:\myPath\a\b\c\file8",
                @"C:\myPath\a\b\c\file9"
            };

            // Sort files to make sure they are processed in the right order.
            Array.Sort(files, customComparer);

            // This list should be in order.
            CollectionAssert.AreEqual(new[] { @"C:\myPath\a\b\c\file1", @"C:\myPath\a\b\c\file2", @"C:\myPath\a\b\c\file3", @"C:\myPath\a\b\c\file4", @"C:\myPath\a\b\c\file5", @"C:\myPath\a\b\c\file6", @"C:\myPath\a\b\c\file7", @"C:\myPath\a\b\c\file8", @"C:\myPath\a\b\c\file9", @"C:\myPath\a\b\c\file10", @"C:\myPath\a\b\c\file11", @"C:\myPath\a\b\c\file12" }, files);
        }
    }
}
