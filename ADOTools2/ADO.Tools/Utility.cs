using System;
using System.Diagnostics;
using System.IO;

namespace ADO.Tools
{
    public static class Utility
    {
        #region - Static Declarations

        #region - Private Members.

        // Create a trace source for the program launcher itself.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("ADO.Tools.Utility"));

        #endregion

        #region - Public Members.

        public static string Base64Encode(string plainText)
        {
            Byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var lengthMod4 = base64EncodedData.Length % 4;
            if (lengthMod4 != 0)
            {
                //fix Invalid length for a Base-64 char array or string
                base64EncodedData += new string('=', 4 - lengthMod4);
            }
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        /// <summary>
        /// Get security identifier (SID) from descriptor.
        /// </summary>
        /// <returns>string</returns>
        public static string GetSidFromDescriptor(string descriptor)
        {
            // Initialize.
            string decodedDescriptor;

            if (descriptor.StartsWith("vssgp."))
            {
                // Extract the encoded descriptor.
                descriptor = descriptor.Substring("vssgp.".Length);

                // Try to perform a base 64 decoding.
                try
                {
                    decodedDescriptor = ADO.Tools.Utility.Base64Decode(descriptor);
                }
                catch (FormatException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
                throw (new NotImplementedException($"unable to handle descriptor [{descriptor}]"));

            // Return descriptor.
            return decodedDescriptor;
        }

        public static void OutputAllText(string path)
        {
            var content = File.ReadAllText(path);

            // Send some traces.
            _mySource.Value.TraceInformation(content);
            _mySource.Value.Flush();
        }

        #endregion

        #endregion
    }
}
