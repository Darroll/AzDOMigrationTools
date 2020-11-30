using System;
using System.Globalization;

namespace VstsSyncMigrator.Core
{
    class Utility
    {
        private static readonly string EnvironmentVariablePrefix = "Environment.";
        public static string GenerateMigrationGreeting(bool reprocessing = false)
        {
            // Initialize.
            string msg;
            string baseMsg = "Migrated by Azure DevOps Migration Tools a fork of <a href='https://dev.azure.com/nkdagility/migration-tools/'>nkdagility VSTS Sync Migration Tools</a>";
            string baseReprocessingMsg = "Migrated (reprocessed) by Azure DevOps Migration Tools a fork of <a href='https://dev.azure.com/nkdagility/migration-tools/'>nkdagility VSTS Sync Migration Tools</a>";

            if (reprocessing)
                msg = baseReprocessingMsg;
            else
                msg = baseMsg;
            
            // Return the greeting message.
            return msg;
        }

        public static UInt32 GenerateTrueRandomInteger(UInt32 minValue, UInt32 maxValue)
        {
            // Initialize.
            UInt32 randomNumber = 0;
            UInt32 diffValue = maxValue - minValue + 1;

            if (minValue < maxValue)
            {
                // Initialize.
                System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();

                try
                {
                    byte[] numberAsBytes = new byte[] { 1, 2, 3, 4 };

                    // Generate the number.
                    rng.GetBytes(numberAsBytes);

                    // Get the number.
                    UInt32 n = System.BitConverter.ToUInt32(numberAsBytes, 0);
                    randomNumber = n % diffValue + minValue;
                }
                finally
                {
                    rng.Dispose();
                }
            }
            
            // Return random number.
            return randomNumber;
        }

        public static bool ValidateIfNumeric(string inputValue, NumberStyles style)
        {
            return Double.TryParse(inputValue, style, CultureInfo.CurrentCulture, out _);
        }

        public static string LoadFromEnvironmentVariables(string rawValue)
        {
            if (!string.IsNullOrEmpty(rawValue))
            {
                if (rawValue.StartsWith(EnvironmentVariablePrefix))
                {
                    string variable = rawValue.Substring(EnvironmentVariablePrefix.Length);
                    var value = Environment.GetEnvironmentVariable(variable);
                    if (!string.IsNullOrEmpty(value))
                    {
                        return value;
                    }
                    else
                    {
                        throw new ArgumentNullException($"{rawValue} should point to an existing environment variable that is non-null");
                    }
                }
                else
                {
                    throw new ArgumentException($"{rawValue} should start with '{EnvironmentVariablePrefix}'");
                }
            }
            else
            {
                return rawValue;
            }
        }
    }
}
