using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ADO.Tools;

namespace ADO.Extensions
{
    public static class EnumExtensions
    {
        #region - Private Members

        //private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("ADO.Extensions.EnumExtensions"));

        #endregion

        #region - Public Members

        /// <summary>
        /// Retrieve the value defined from DescriptionAttribute from an enumeration value.
        /// </summary>
        /// <returns>string</returns>
        public static string GetDescription(this Enum value)
        {
            // Initialize.
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            string description = null;

            // If a name is defined, proceed...
            if (name != null)
            {
                // Try to get field metadata.
                System.Reflection.FieldInfo field = type.GetField(name);

                // If field metadata is defined, proceed.
                if (field != null)
                {
                    // Extract custom attribute from the field.
                    if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attr)
                        description = attr.Description;
                }
            }

            // Return description if any.
            return description;
        }

        #endregion
    }

    public static class JTokenExtensions
    {
        #region - Private Members

        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("ADO.Extensions.JTokenExtensions"));

        #endregion

        #region - Public Members

        /// <summary>
        /// Get computed MD5 hash from the path of JSON token.
        /// </summary>
        /// <returns>string</returns>
        public static string GetTokenPathHash(this JToken token)
        {
            // Initialize.
            byte[] hashBuffer;
            StringBuilder sb = new StringBuilder();

            using (MD5 md5 = MD5.Create())
            {
                md5.Initialize();
                md5.ComputeHash(Encoding.UTF8.GetBytes(token.Path));
                hashBuffer = md5.Hash;
            }

            // Combine the buffer and form a hexadecimal string.
            for (int i = 0; i < hashBuffer.Length; i++)
                sb.Append(hashBuffer[i].ToString("X2"));

            // Return the hash;
            return sb.ToString();
        }

        /// <summary>
        /// Remove nodes from a JSON document based only on the property name.
        /// </summary>
        /// <remark>This code is inspired by answers found <a href="https://stackoverflow.com/questions/11676159/json-net-how-to-remove-nodes">here</a>.</remark>
        /// <returns>JToken</returns>
        public static JToken RemoveProperties(this JToken token, string[] propertyNames)
        {
            // Do nothing if a container has been reached.
            if (!(token is JContainer container)) return token;

            // Initialize.
            List<JToken> removalList = new List<JToken>();

            foreach (JToken j in container.Children())
            {
                // Add to list for removal.
                if (j is JProperty p && propertyNames.Contains(p.Name))
                    removalList.Add(j);

                // Remove fields on current token.
                j.RemoveProperties(propertyNames);
            }

            // Remove any items found in that list.
            foreach (JToken j in removalList)
                j.Remove();

            // Return token.
            return token;
        }

        /// <summary>
        /// Remove nodes from a JSON document based on JSONPath syntax.
        /// If you need help on how to form JSONPath consult <a href="https://restfulapi.net/json-jsonpath/">here</a> for more information.
        /// you can also look <a href="https://github.com/json-path/JsonPath">at this GitHub repository</a> for more.
        /// A useful tester can be found <a href="https://codebeautify.org/jsonpath-tester">here</a>.
        /// </summary>        
        /// <returns>JToken</returns>
        /// <example> To remove 6 properties at the root of the JSON document structure illustrated below:
        /// <br/>
        /// <code>
        /// {
        ///   "runsOn": [ "Agent", "DeploymentGroup"],
        ///   "comment": "",
        ///   "id": "something",
        ///   "definitionType": "something",
        ///   "revision": "something",
        ///   "name": "PIPowerShellTestDeployCleanup - Manual Test",
        ///   "version": { "major": 1, "minor": 0, "patch": 0, "isTest": false },
        ///   "iconUrl": "https://cdn.vsassets.io/v/20180117T135113/_content/icon-meta-task.png",
        ///   "friendlyName": "PIPowerShellTestDeployCleanup - Manual Test",
        ///   "description": "Copy necessary files and run filter generator and Deploy/Cleanup"
        /// }
        /// </code>        
        /// <code>
        /// pathsToSelectForRemoval = new string[]
        ///     { "$.id", "$.definitionType", "$.revision" };        
        /// var curatedJToken = oneJToken.RemoveSelectProperties(pathsToSelectForRemoval);
        /// </code>
        /// <br/>
        /// The JSON document will now be like below:
        /// <code>
        /// {
        ///   "runsOn": [ "Agent", "DeploymentGroup"],
        ///   "comment": "",        
        ///   "name": "PIPowerShellTestDeployCleanup - Manual Test",
        ///   "version": { "major": 1, "minor": 0, "patch": 0, "isTest": false },
        ///   "iconUrl": "https://cdn.vsassets.io/v/20180117T135113/_content/icon-meta-task.png",
        ///   "friendlyName": "PIPowerShellTestDeployCleanup - Manual Test",
        ///   "description": "Copy necessary files and run filter generator and Deploy/Cleanup"
        /// }
        /// </code>
        /// </example>
        public static JToken RemoveSelectProperties(this JToken token, string[] paths)
        {
            // Initialize.
            List<string> hashesList = new List<string>();

            // Search all tokens matching these paths.
            // Add to list of hash JToken paths.
            foreach (string path in paths)
            {
                // Retrieve all JTokens matching the JsonPath.
                var result = token.SelectTokens(path);

                // Add a null when no JTokens were found.
                if (result == null)
                    hashesList.Add(null);
                else
                    foreach (JToken j in result)
                        hashesList.Add(j.GetTokenPathHash());
            }

            // Remove tokens using the JToken path hashes.
            token.RemoveTokenByPathHash(hashesList.ToArray());

            // Return token.
            return token;
        }

        /// <summary>
        /// Rename properties from a JSON document based on JSONPath syntax.
        /// If you need help on how to form JSONPath consult <a href="https://restfulapi.net/json-jsonpath/">here</a> for more information.
        /// </summary>        
        /// <param name="paths">Array of JSONPaths where a JSON token must be removed</param>
        /// <param name="newNames">Array of names to replace the actual property names matching the JSONPaths</param>
        /// <returns>JToken</returns>
        public static JToken RenameSelectProperties(this JToken token, string[] paths, string[] newNames)
        {
            // Validate conditions.
            if (paths.Length != newNames.Length)
                throw (new ArgumentException("The number of paths must match the number of property names"));

            // Initialize.
            List<string> hashesFound;
            Dictionary<string, string[]> pathHashesTable = new Dictionary<string, string[]>();

            // Search all tokens matching these paths.
            // Add to list of hash JToken paths.
            foreach (string path in paths)
            {
                // Instantiate.
                hashesFound = new List<string>();

                // Retrieve all JTokens matching the JsonPath.
                foreach (JToken j in token.SelectTokens(path))
                    hashesFound.Add(j.GetTokenPathHash());

                // Add to dictionary.
                pathHashesTable.Add(path, hashesFound.ToArray());
            }

            // Rename properties using the JToken path hashes.
            token.RenamePropertyByPathHash(paths, newNames, pathHashesTable);

            // Return token.
            return token;
        }

        /// <summary>
        /// Remove JSON tokens from a JSON document based on computed MD5 hash from the path of JSON tokens.
        /// </summary>        
        /// <param name="hashes">Computed MD5 hashes from paths of JSON tokens to remove</param>
        /// <returns>JToken</returns>
        public static JToken RemoveTokenByPathHash(this JToken token, string[] hashes)
        {
            try
            {
                // Do nothing if a container has been reached.
                if (!(token is JContainer container)) return token;

                // Initialize.
                List<JToken> directRemovalList = new List<JToken>();
                List<JToken> fromParentRemovalList = new List<JToken>();

                foreach (JToken j in container.Children())
                {
                    // Evaluate current token if its path hash is matching.
                    // If true, add to list for direct removal.
                    if (j.Type == JTokenType.Property)
                        if (hashes.Contains(j.GetTokenPathHash()))
                            directRemovalList.Add(j);

                        // When dealing with an object or an array to remove, this needs to
                        // be accomplished from the parent only because at this point we are
                        // dealing only with the a JContainer object from which no deletion is
                        // possible.
                        // If true, add to list for removal from parent.
                        else if (j.Type == JTokenType.Object && j.Parent.Type == JTokenType.Array)
                            if (hashes.Contains(j.GetTokenPathHash()))
                                fromParentRemovalList.Add(j);

                    // Continue the traversal process.
                    RemoveTokenByPathHash(j, hashes);
                }

                // Remove any JProperty found.
                foreach (JToken j in directRemovalList)
                    j.Remove();

                // Remove any JObject under a JArray.
                foreach (JToken j in fromParentRemovalList)
                    ((JArray)j.Parent).Remove(j);
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = "Error occured while manipulating JToken: " + ex.Message;
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();

                throw;
            }

            // Return token.
            return token;
        }

        /// <summary>
        /// Rename properties from a JSON document based on computed MD5 hash from the path of JSON tokens.
        /// </summary>        
        /// <param name="paths">Array of JSONPaths where a JSON token must be renamed</param>
        /// <param name="newNames">Array of names to replace the actual property names matching the JSONPaths</param>
        /// <param name="pathHashesTable">Dictionary of JSONPaths and all computed MD5 hashes from paths of JSON tokens matching the JSONPaths</param>
        /// <returns>JToken</returns>
        public static JToken RenamePropertyByPathHash(this JToken token, string[] paths, string[] newNames, Dictionary<string, string[]> pathHashesTable)
        {
            // Do nothing if a container has been reached.
            if (!(token is JContainer container)) return token;

            // Initialize.
            int index = 0;
            List<object[]> renameInstructionList = new List<object[]>();
            List<string> listOfPaths = paths.ToList();

            foreach (JToken j in container.Children())
            {
                if (j is JProperty p)
                {
                    // Evaluate current token if its path hash is matching.
                    // if true, add to list for removal.
                    foreach (var item in pathHashesTable)
                    {
                        // Find the position in the list where this path exists.
                        index = listOfPaths.FindIndex(x => x == item.Key);

                        foreach (string hash in item.Value)
                        {
                            // If the hash matches...
                            if (hash == j.GetTokenPathHash())
                            {
                                // Store rename instructions which have the JToken to modify and new property name to assign.
                                renameInstructionList.Add(new Object[] { j, newNames[index] });

                                // Stop searching.
                                break;
                            }
                        }
                    }
                }

                // Continue the renaming process.
                RenamePropertyByPathHash(j, paths, newNames, pathHashesTable);
            }

            // Rename any items found in that list.
            foreach (Object[] instruction in renameInstructionList)
                ((JToken)instruction[0]).RenameProperty((String)instruction[1]);

            // Return token.
            return token;
        }

        /// <summary>
        /// Rename a JSON token which is a JProperty.
        /// </summary>                
        public static void RenameProperty(this JToken token, string newPropertyName)
        {
            // Proceed only if token is a JProperty.
            // Replace current token with new token with new property name.
            if (token is JProperty p)
                token.Replace(new JProperty(newPropertyName, p.Value));
            else
            {
                // Raise an exception.
                string errorMsg = string.Format("Cannot rename a property of a {0}", token.Type.ToString());
                throw (new Exception(errorMsg));
            }
        }

        /// <summary>
        /// Select the first token that meets a JSON path.
        /// </summary>      
        public static JToken SelectFirstToken(this JToken token, string[] paths)
        {
            // Initialize.
            JToken j = null;

            // Try every JSON paths.
            foreach (string path in paths)
            {
                // Try to find token with path.
                j = token.SelectToken(path);

                if (j != null)
                    break;
            }

            // Return token found.
            return j;
        }

        #endregion
    }
}