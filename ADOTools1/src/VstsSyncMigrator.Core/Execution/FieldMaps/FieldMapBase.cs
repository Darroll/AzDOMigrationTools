using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace VstsSyncMigrator.Engine.ComponentContext
{
    public abstract class FieldMapBase : IFieldMap
    {
        #region - Static Declarations

        // Create a trace source.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine.FieldMapBase"));

        #endregion

        #region - Protected Members

        protected string ConvertPlaintextToHtmlFormat(string inputValue)
        {
            // Initialize.            
            string searchForSpacesTemplate = @"[ ]{{{0}}}";
            string searchForHtmlLikeTag = @"<[^>]*>";
            string htmlLine = @"<br/>";
            string htmlApostrophe = @"&apos;";
            string htmlQuote = @"&quot;";
            string htmlAmpersand = @"&amp;";
            string htmlSpace = @"&nbsp;";
            string htmlgt = @"&gt;";
            string htmllt = @"&lt;";
            MatchCollection matches;

            string dynamicExpr;
            string e1;
            string e2;
            string replacementValue;
            Dictionary<string, string> replacementDict = new Dictionary<string, string>();

            // Perform a standard replace of & with '&amp;'.
            inputValue = inputValue.Replace("&", htmlAmpersand);

            matches = Regex.Matches(inputValue, searchForHtmlLikeTag);
            foreach (Match match in matches)
            {
                // Remove '<' and '>'.
                e1 = match.Value;
                e2 = match.Value.Substring(1, e1.Length - 2);

                // Create a dynamic expression with the html-like tag.
                dynamicExpr = "<" + Regex.Escape(e2) + ">"; ;
                replacementValue = htmllt + e2 + htmlgt;
                if (!replacementDict.ContainsKey(dynamicExpr))
                    replacementDict.Add(dynamicExpr, replacementValue);
            }

            // Find all places where 2+ spaces and store within a dictionary.
            dynamicExpr = string.Format(searchForSpacesTemplate, @"2,");
            matches = Regex.Matches(inputValue, dynamicExpr);
            foreach (Match match in matches)
            {
                // Create a dynamic expression with the number of spaces.
                dynamicExpr = string.Format(searchForSpacesTemplate, match.Value.Length);
                replacementValue = string.Concat(Enumerable.Repeat(htmlSpace, match.Value.Length));
                if (!replacementDict.ContainsKey(dynamicExpr))
                    replacementDict.Add(dynamicExpr, replacementValue);
            }

            // Perform all replacements.
            foreach (var item in replacementDict)
                inputValue = Regex.Replace(inputValue, item.Key, item.Value);

            // Perform a standard replace of " with '&quot;'.
            // Perform a standard replace of ' with '&apos;'.
            // Perform a standard replace tab with 4 x '&nbsp;'.
            // Perform a standard replace of CR/LF or LF with '<br>'.
            inputValue = inputValue.Replace("'", htmlApostrophe);
            inputValue = inputValue.Replace("\"", htmlQuote);
            inputValue = inputValue.Replace("\t", string.Concat(Enumerable.Repeat(htmlSpace, 4)));
            inputValue = inputValue.Replace("\r\n", htmlLine).Replace("\n", htmlLine);

            // Return modified value.
            return inputValue;
        }

        protected string RemovePlaintextRemnants(string inputValue)
        {
            // Initialize.            
            string searchForSpacesTemplate = @"[ ]{{{0}}}";
            string htmlLine = @"<br/>";
            string htmlSpace = @"&nbsp;";
            string dynamicExpr;
            string replacementValue;
            MatchCollection matches;
            Dictionary<string, string> replacementDict = new Dictionary<string, string>();

            // Find all places where 2+ spaces and store within a dictionary.
            dynamicExpr = string.Format(searchForSpacesTemplate, @"2,");
            matches = Regex.Matches(inputValue, dynamicExpr);
            foreach (Match match in matches)
            {
                // Create a dynamic expression with the number of spaces.
                dynamicExpr = string.Format(searchForSpacesTemplate, match.Value.Length);
                replacementValue = string.Concat(Enumerable.Repeat(htmlSpace, match.Value.Length));
                if (!replacementDict.ContainsKey(dynamicExpr))
                    replacementDict.Add(dynamicExpr, replacementValue);
            }

            // Perform all replacements.
            foreach (var item in replacementDict)
                inputValue = Regex.Replace(inputValue, item.Key, item.Value);

            // Perform a standard replace tab with 4 x '&nbsp;'.
            // Perform a standard replace or CR/LF or LF with '<br>'.
            inputValue = inputValue.Replace("\t", string.Concat(Enumerable.Repeat(htmlSpace, 4)));
            inputValue = inputValue.Replace("\r\n", htmlLine).Replace("\n", htmlLine);

            // Return modified value.
            return inputValue;
        }

        protected abstract void InternalExecute(WorkItem sourceWI, WorkItem targetWI);

        #endregion

        #region - Public Members

        public abstract string MappingDisplayName { get; }

        public string Name
        {
            get { return this.GetType().Name; }
        }

        public void Execute(WorkItem sourceWI, WorkItem targetWI)
        {
            try
            {
                InternalExecute(sourceWI, targetWI);
            }
            catch (Exception ex)
            {
                // Send telemetry data.
                Telemetry.Current.TrackException(
                        ex,
                        new Dictionary<string, string> {
                            { "Source", sourceWI.Id.ToString() },
                            { "Target",  targetWI.Id.ToString()}
                       }
                    );

                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, $"[EXCEPTION] {ex}");
                _mySource.Value.Flush();
            }            
        }

        #endregion
    }
}