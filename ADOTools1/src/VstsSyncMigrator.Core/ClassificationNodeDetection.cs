using System.Collections.Generic;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.Server;

namespace VstsSyncMigrator.Engine
{
    public class ClassificationNodeDetection
    {
        #region - Private Members

        private readonly ICommonStructureService _css;
        private readonly List<string> _foundNodes = new List<string>();
        private readonly WorkItemStore _wiStore;

        #endregion

        #region - Public Members

        public ClassificationNodeDetection(WorkItemStore store)
        {
            // Define work item store to utilize.
            _wiStore = store;

            // Cache connection to common structure service.
            if (_css == null)
                _css = (ICommonStructureService4)store.TeamProjectCollection.GetService(typeof(ICommonStructureService4));
        }

        public bool NodeExists(string nodePath)
        {
            // Initialize.
            bool value = true;

            if (!_foundNodes.Contains(nodePath))
            {
                // Format of the path is: \ProjectName\RootNodeName\NodeNameParent\NodeName
                try
                {
                    NodeInfo node = _css.GetNodeFromPath(nodePath);

                    // Add path to cache.
                    _foundNodes.Add(nodePath);
                }
                catch
                { value = false; }
            }

            // Return the answer.
            return value;
        }

        #endregion
    }
}