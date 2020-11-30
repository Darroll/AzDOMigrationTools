using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace VstsSyncMigrator.Engine.ComponentContext
{
   public interface IFieldMap
    {
        string Name { get; }

        string MappingDisplayName { get; }

        void Execute(WorkItem source, WorkItem target);
    }
}
