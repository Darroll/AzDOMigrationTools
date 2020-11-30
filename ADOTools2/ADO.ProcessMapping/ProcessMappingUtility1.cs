using ADO.RestAPI.ProcessMapping;
using ADO.RestAPI.Viewmodel50;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ADO.ProcessMapping
{
    public static class ProcessMapUtility1
    {
        public static void ApplyProcessMapping(
            Dictionary<string, string> workItemTypeDefinition,
            string sourceProcessTypeName,
            string destinationProcessTypeName,
            Maps maps)
        {
            if (string.Equals(sourceProcessTypeName, destinationProcessTypeName,
                StringComparison.OrdinalIgnoreCase))
            {
                //no process mapping to do
            }
            else
            {
                InheritedProcess sourceProcess = maps.InheritedProcessDictionary[sourceProcessTypeName];
                InheritedProcess targetProcess = maps.InheritedProcessDictionary[destinationProcessTypeName];

                ProcessMap processMap = maps.GetBestProcessMap(sourceProcessTypeName, destinationProcessTypeName);
                if (processMap != null)
                {
                    foreach (var wit in processMap.NonTrivialWorkItemTypeMap)
                    {
                        var sourceWit = sourceProcess.WorkItemTypes.Single(w => w.Id == wit.Key);
                        var targetWit = targetProcess.WorkItemTypes.Single(w => w.Id == wit.Value);

                        workItemTypeDefinition[sourceWit.Name] = targetWit.Name;
                    }
                }
                else
                    throw new NotImplementedException($"bulk import from {sourceProcessTypeName} to {destinationProcessTypeName}. Please add it to maps");
            }
        }

        public static void ApplyProcessMappingFieldMap(
            List<VstsSyncMigrator.Engine.Configuration.FieldMap.IFieldMapConfig> fieldMaps,
            string sourceProcessTypeName,
            string destinationProcessTypeName,
            Maps maps)
        {
            if (string.Equals(sourceProcessTypeName, destinationProcessTypeName,
                StringComparison.OrdinalIgnoreCase))
            {
                //no process mapping to do
            }
            else
            {
                InheritedProcess sourceProcess = maps.InheritedProcessDictionary[sourceProcessTypeName];
                //InheritedProcess targetProcess = maps.InheritedProcessDictionary[destinationProcessTypeName];

                ProcessMap processMap = maps.GetBestProcessMap(sourceProcessTypeName, destinationProcessTypeName);
                if (processMap != null)
                {
                    foreach (var wit in processMap.NonTrivialWorkItemTypeFieldMap)
                    {
                        var sourceWit = sourceProcess.WorkItemTypes.Single(w => w.Id == wit.Key);


                        //https://mohamedradwan.com/2017/09/15/tfs-2017-migration-to-vsts-with-vsts-sync-migrator/
                        foreach (var innerKey in wit.Value)
                        {

                            fieldMaps.Add(
                                new VstsSyncMigrator.Engine.Configuration.FieldMap.FieldtoFieldMapConfig()
                                {
                                    WorkItemTypeName = sourceWit.Name,// "User Story",
                                    SourceField = innerKey.Key,//"Microsoft.VSTS.Scheduling.StoryPoints",
                                    TargetField = innerKey.Value,//"Microsoft.VSTS.Scheduling.Size"
                                }
                                );
                        }
                    }
                    foreach (var wit in processMap.NonTrivialWorkItemTypeStateMap)
                    {
                        var sourceWit = sourceProcess.WorkItemTypes.Single(w => w.Id == wit.Key);


                        //https://mohamedradwan.com/2017/09/15/tfs-2017-migration-to-vsts-with-vsts-sync-migrator/

                        fieldMaps.Add(
                            new VstsSyncMigrator.Engine.Configuration.FieldMap.FieldValueMapConfig()
                            {
                                WorkItemTypeName = sourceWit.Name,// "*",
                                SourceField = "System.State",
                                TargetField = "System.State",
                                ValueMapping = wit.Value
                            }
                            );
                    }
                }
                else
                {
                    throw new NotImplementedException($"bulk import from {sourceProcessTypeName} to {destinationProcessTypeName}. Please add it to maps");
                }
            }
        }
    }
}
