using System;
using System.Text.RegularExpressions;

namespace ADO.Engine.DefinitionFiles
{
    public abstract class BaseDefinitionFile
    {
        public string FilePath { get; protected set; }

        public string FileName
        {
            get { return string.IsNullOrEmpty(this.FilePath) ? string.Empty : System.IO.Path.GetFileName(this.FilePath); }
        }

        public virtual void AddDefinition(string path)
        {
            FilePath = path;
        }
    }

    public class MultiObjectsDefinitionFile : BaseDefinitionFile
    {
        // This class is not different than the base class for now.
    }

    public class PullRequestDefinitionFiles
    {
        public SingleObjectDefinitionFile PullRequest { get; set; }

        public MultiObjectsDefinitionFile PullRequestThreads { get; set; }
    }

    public class SingleObjectDefinitionFile : BaseDefinitionFile
    {
        public int FileIdentifier { get; private set; }

        public override void AddDefinition(string path)
        {
            // Initialize.
            Regex rx = new Regex(@"\d+", RegexOptions.IgnoreCase);
            string value = null;

            // Assign path.
            FilePath = path;
            
            // Find matches.
            MatchCollection matches = rx.Matches(System.IO.Path.GetFileName(path));

            if (matches.Count > 0)
            {
                // Care only about the first value found.
                foreach (Match match in matches)
                {
                    value = match.Value;
                    break;
                }

                // Convert to abstract identifier.
                this.FileIdentifier = Convert.ToInt32(value);
            }
        }
    }

    public class TeamConfigurationDefinitionFiles
    {
        public SingleObjectDefinitionFile Areas { get; set; }

        public SingleObjectDefinitionFile BoardColumns { get; set; }

        public SingleObjectDefinitionFile BoardRows { get; set; }

        public SingleObjectDefinitionFile CardFields { get; set; }

        public SingleObjectDefinitionFile CardStyles { get; set; }

        public SingleObjectDefinitionFile Iterations { get; set; }

        public SingleObjectDefinitionFile Settings { get; set; }

        public string TeamName { get; set; }
    }    
}