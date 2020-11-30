using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using VstsSyncMigrator.Engine.ComponentContext;
using VstsSyncMigrator.Engine.Configuration.FieldMap;

namespace VstsSyncMigrator.Engine
{
    public class TreeToTagFieldMap : IFieldMap
    {
        #region - Private Members

        private readonly TreeToTagMapConfig _config;

        #endregion

        #region - Public Members

        public string MappingDisplayName => string.Empty;

        public string Name
        {
            get { return "TreeToTagFieldMap"; }
        }

        public TreeToTagFieldMap(TreeToTagMapConfig config)
        {
            _config = config;
        }

        public void Execute(WorkItem sourceWI, WorkItem targetWI)
        {
            // Initialize.
            string value;
            List<string> listOfNewTags = targetWI.Tags.Split(char.Parse(@";")).ToList();

            if (_config.TimeTravel > 0)
                value = (string)sourceWI.Revisions[sourceWI.Revision - _config.TimeTravel].Fields["System.AreaPath"].Value;
            else
                value = sourceWI.AreaPath;

            List<string> bits = new List<string>(value.Split(char.Parse(@"\"))).Skip(_config.ToSkip).ToList();
            targetWI.Tags = string.Join(";", listOfNewTags.Union(bits).ToArray());
        }

        #endregion
    }
}