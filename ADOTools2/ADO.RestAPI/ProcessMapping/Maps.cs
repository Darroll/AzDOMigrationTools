using ADO.RestAPI.Viewmodel50;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ADO.RestAPI.ProcessMapping
{
    public class Maps
    {
        public string InheritedProcessSourcePath { get; set; }

        public List<ProcessMap> ProcessMaps { get; set; } = new List<ProcessMap>();

        [JsonIgnore()]
        public Dictionary<string, InheritedProcess> InheritedProcessDictionary { get; set; } = new Dictionary<string, InheritedProcess>();


        public void LoadAllProcessesFromFolder(string path)
        {
            var allProcesses = Directory.EnumerateFiles(path);
            foreach (var item in allProcesses)
            {
                var filename = Path.GetFileNameWithoutExtension(item);
                string json = File.ReadAllText(item);
                InheritedProcess inheritedProcess = JsonConvert.DeserializeObject<InheritedProcess>(json);
                this.InheritedProcessDictionary.Add(filename, inheritedProcess);
            }
        }

        public ProcessMap GetProcessMap(string sourceProcess, string targetProcess)
        {
            return this.ProcessMaps.SingleOrDefault(pm => pm.SourceProcess == sourceProcess && pm.TargetProcess == targetProcess);
        }

        public Maps()
        {

        }

        public Maps(string path)
        {
            this.InheritedProcessSourcePath = new FileInfo(path).FullName;
            this.LoadAllProcessesFromFolder(path);
            bool isValid = this.Validate();
            if (!isValid)
            {
                throw new ArgumentException("invalid");
            }
        }

        public ProcessMap PopulateBestPossibleProcessMap(string sourceProcess, string targetProcess, bool resetUnmappedCollections, bool generateNonTrivialMaps, bool resetBoardMaps)
        {
            if (this.InheritedProcessDictionary.ContainsKey(sourceProcess) && this.InheritedProcessDictionary.ContainsKey(targetProcess))
            {
                InheritedProcess sourceInheritedProcess = this.InheritedProcessDictionary[sourceProcess];
                InheritedProcess targetInheritedProcess = this.InheritedProcessDictionary[targetProcess];
                ProcessMap processMap
                    = this.GetBestProcessMap(sourceProcess, targetProcess);
                if (processMap != null)
                {
                    if (processMap.WorkItemTypeMap == null)
                        processMap.WorkItemTypeMap = new Dictionary<string, string>();

                    if (processMap.WorkItemTypeFieldMap == null)
                        processMap.WorkItemTypeFieldMap = new Dictionary<string, Dictionary<string, string>>();

                    if (processMap.WorkItemTypeStateMap == null)
                        processMap.WorkItemTypeStateMap = new Dictionary<string, Dictionary<string, string>>();

                    if (processMap.WorkItemTypeBoardMap == null || resetBoardMaps)
                        processMap.WorkItemTypeBoardMap = new Dictionary<string, KeyValuePair<string, string>>();

                    if (processMap.UnmappedSourceWorkItemTypes == null || resetUnmappedCollections)
                        processMap.UnmappedSourceWorkItemTypes = new HashSet<string>();
                    if (processMap.UnmappedSourceWorkItemTypeFieldMaps == null || resetUnmappedCollections)
                        processMap.UnmappedSourceWorkItemTypeFieldMaps = new Dictionary<string, HashSet<string>>();
                    if (processMap.UnmappedSourceWorkItemTypeStateMaps == null || resetUnmappedCollections)
                        processMap.UnmappedSourceWorkItemTypeStateMaps = new Dictionary<string, HashSet<string>>();
                    if (processMap.UnmappedSourceWorkItemTypeBoardMaps == null || resetUnmappedCollections)
                        processMap.UnmappedSourceWorkItemTypeBoardMaps = new HashSet<string>();
                }
                else
                {
                    processMap

                = new ProcessMap()
                {
                    SourceProcess = sourceProcess,
                    TargetProcess = targetProcess,
                    WorkItemTypeMap = new Dictionary<string, string>(),
                    WorkItemTypeFieldMap = new Dictionary<string, Dictionary<string, string>>(),
                    WorkItemTypeStateMap = new Dictionary<string, Dictionary<string, string>>(),
                    WorkItemTypeBoardMap = new Dictionary<string, KeyValuePair<string, string>>(),
                    UnmappedSourceWorkItemTypes = new HashSet<string>(),
                    UnmappedSourceWorkItemTypeFieldMaps = new Dictionary<string, HashSet<string>>(),
                    UnmappedSourceWorkItemTypeStateMaps = new Dictionary<string, HashSet<string>>(),
                    UnmappedSourceWorkItemTypeBoardMaps = new HashSet<string>(),
                };
                }

                foreach (var wit in sourceInheritedProcess.WorkItemTypes
                    .Select(w => w.Id).Except(processMap.WorkItemTypeMap.Keys))
                {
                    if (targetInheritedProcess.WorkItemTypes.Any(w => w.Id == wit))
                    {
                        processMap.WorkItemTypeMap.Add(wit, wit);
                    }
                    else
                    {
                        processMap.UnmappedSourceWorkItemTypes.Add(wit);
                    }
                }

                //board map
                foreach (var sourcewit in sourceInheritedProcess.WorkItemTypes
                    .Select(w => w.Id)
                    .Except(processMap.WorkItemTypeBoardMap.Keys)
                    )
                {
                    if (processMap.WorkItemTypeMap.ContainsKey(sourcewit))
                    {
                        string targetwit = processMap.WorkItemTypeMap[sourcewit];
                        processMap.WorkItemTypeBoardMap.Add(sourcewit, GetBestPossibleBoardMap(sourcewit, targetwit, sourceProcess, targetProcess));
                    }
                    else
                    {
                        //source wit not yet mapped -- no point in trying to map corresponding board
                        processMap.UnmappedSourceWorkItemTypeBoardMaps.Add(sourcewit);
                    }
                }

                //now loop for fields
                foreach (var sourceWorkItemTypeField in sourceInheritedProcess.WorkItemTypeFields
                    .Where(wf => processMap.WorkItemTypeMap.ContainsKey(wf.WorkItemTypeRefName)))
                {
                    var targetWorkItemTypeField = targetInheritedProcess.WorkItemTypeFields
                        .Single(witf => witf.WorkItemTypeRefName == processMap.WorkItemTypeMap[sourceWorkItemTypeField.WorkItemTypeRefName]);

                    List<Field1> sourceWorkItemTypeFieldFieldsToMap = null;
                    if (processMap.WorkItemTypeFieldMap.ContainsKey(sourceWorkItemTypeField.WorkItemTypeRefName))
                    {
                        sourceWorkItemTypeFieldFieldsToMap = sourceWorkItemTypeField.Fields
                            .Where(bb => !processMap.WorkItemTypeFieldMap[sourceWorkItemTypeField.WorkItemTypeRefName].Keys.Contains(bb.ReferenceName))
                            .ToList();
                    }
                    else
                    {
                        //all fields must be mapped
                        sourceWorkItemTypeFieldFieldsToMap = sourceWorkItemTypeField.Fields;
                    }
                    foreach (var sourceWitField in sourceWorkItemTypeFieldFieldsToMap)
                    {
                        if (targetWorkItemTypeField.Fields.Any(tt => tt.ReferenceName == sourceWitField.ReferenceName))
                        {
                            if (processMap.WorkItemTypeFieldMap.ContainsKey(sourceWorkItemTypeField.WorkItemTypeRefName))
                            {
                                processMap.WorkItemTypeFieldMap[sourceWorkItemTypeField.WorkItemTypeRefName]
                                    .Add(sourceWitField.ReferenceName, sourceWitField.ReferenceName);
                            }
                            else
                            {
                                processMap.WorkItemTypeFieldMap.Add(sourceWorkItemTypeField.WorkItemTypeRefName,
                                    new Dictionary<string, string>() { { sourceWitField.ReferenceName, sourceWitField.ReferenceName } });
                            }
                        }
                        else
                        {
                            if (processMap.UnmappedSourceWorkItemTypeFieldMaps.ContainsKey(sourceWorkItemTypeField.WorkItemTypeRefName))
                            {
                                processMap.UnmappedSourceWorkItemTypeFieldMaps[sourceWorkItemTypeField.WorkItemTypeRefName]
                                    .Add(sourceWitField.ReferenceName);
                            }
                            else
                            {
                                processMap.UnmappedSourceWorkItemTypeFieldMaps.Add(
                                    sourceWorkItemTypeField.WorkItemTypeRefName, new HashSet<string>() { sourceWitField.ReferenceName });
                            }
                        }
                    }
                }

                //also states
                foreach (var sourceWorkItemTypeState in sourceInheritedProcess.States
                    .Where(wf => processMap.WorkItemTypeMap.ContainsKey(wf.WorkItemTypeRefName)))
                {
                    var targetWorkItemTypeState = targetInheritedProcess.States
                        .Single(witf => witf.WorkItemTypeRefName == processMap.WorkItemTypeMap[sourceWorkItemTypeState.WorkItemTypeRefName]);

                    List<State1> sourceWorkItemTypeStateStatesToMap = null;
                    if (processMap.WorkItemTypeStateMap.ContainsKey(sourceWorkItemTypeState.WorkItemTypeRefName))
                    {
                        sourceWorkItemTypeStateStatesToMap = sourceWorkItemTypeState.States
                            .Where(bb => !processMap.WorkItemTypeStateMap[sourceWorkItemTypeState.WorkItemTypeRefName].Keys.Contains(bb.Name))
                            .ToList();
                    }
                    else
                    {
                        //all States must be mapped
                        sourceWorkItemTypeStateStatesToMap = sourceWorkItemTypeState.States;
                    }
                    foreach (var sourceWitState in sourceWorkItemTypeStateStatesToMap)
                    {
                        if (targetWorkItemTypeState.States.Any(tt => tt.Name == sourceWitState.Name))
                        {
                            if (processMap.WorkItemTypeStateMap.ContainsKey(sourceWorkItemTypeState.WorkItemTypeRefName))
                            {
                                processMap.WorkItemTypeStateMap[sourceWorkItemTypeState.WorkItemTypeRefName]
                                    .Add(sourceWitState.Name, sourceWitState.Name);
                            }
                            else
                            {
                                processMap.WorkItemTypeStateMap.Add(sourceWorkItemTypeState.WorkItemTypeRefName,
                                    new Dictionary<string, string>() { { sourceWitState.Name, sourceWitState.Name } });
                            }
                        }
                        else
                        {
                            if (processMap.UnmappedSourceWorkItemTypeStateMaps.ContainsKey(sourceWorkItemTypeState.WorkItemTypeRefName))
                            {
                                processMap.UnmappedSourceWorkItemTypeStateMaps[sourceWorkItemTypeState.WorkItemTypeRefName]
                                    .Add(sourceWitState.Name);
                            }
                            else
                            {
                                processMap.UnmappedSourceWorkItemTypeStateMaps.Add(
                                    sourceWorkItemTypeState.WorkItemTypeRefName, new HashSet<string>() { sourceWitState.Name });
                            }
                        }
                    }
                }

                if (generateNonTrivialMaps)
                {
                    processMap.NonTrivialWorkItemTypeMap = processMap.WorkItemTypeMap.Where(ent => ent.Key != ent.Value).ToDictionary(k => k.Key, v => v.Value);
                    processMap.NonTrivialWorkItemTypeFieldMap
                        = processMap.WorkItemTypeFieldMap.Where(ent => ent.Value.Any(a => a.Key != a.Value))
                        .ToDictionary(k => k.Key, v => v.Value.Where(b => b.Key != b.Value).ToDictionary(innerk => innerk.Key, innerv => innerv.Value));
                    processMap.NonTrivialWorkItemTypeStateMap
                        = processMap.WorkItemTypeStateMap.Where(ent => ent.Value.Any(a => a.Key != a.Value))
                        .ToDictionary(k => k.Key, v => v.Value.Where(b => b.Key != b.Value).ToDictionary(innerk => innerk.Key, innerv => innerv.Value));
                    processMap.NonTrivialWorkItemTypeBoardMap = processMap.WorkItemTypeBoardMap.Where(ent => ent.Value.Key != ent.Value.Value).ToDictionary(k => k.Key, v => v.Value);
                }


                return processMap;
            }
            else
            {
                throw new ArgumentException($"couldn't find process {sourceProcess} or {targetProcess}");
            }
        }

        public string GetParentProcess(string sourceProcessType)
        {
            var process = GetProcess(sourceProcessType);
            return GetSystemProcess(process.Process.Properties.ParentProcessTypeId);
        }

        public string GetSystemProcess(string parentProcessTypeId)
        {
            if (parentProcessTypeId == Constants.Agile)
                return Constants.AgileTemplateType;
            else if (parentProcessTypeId == Constants.SCRUM)
                return Constants.ScrumTemplateType;
            else if (parentProcessTypeId == Constants.CMMI)
                return Constants.CmmiTemplateType;
            else
            {
                throw new ArgumentException($"unknown template base id: {parentProcessTypeId}");
            }
        }

        public InheritedProcess GetProcess(string sourceProcessType)
        {
            if (this.InheritedProcessDictionary.ContainsKey(sourceProcessType))
            {
                return this.InheritedProcessDictionary[sourceProcessType];
            }
            else
            {
                throw new KeyNotFoundException($"could not find process {sourceProcessType} in maps {this.InheritedProcessSourcePath}. Please ensure it exists");
            }
        }

        public KeyValuePair<string, string> GetBestPossibleBoardMap(string sourcewit, string targetwit, string sourceProcess, string targetProcess)
        {
            string key = GetBestPossibleBoardName(sourcewit, sourceProcess);
            string value = GetBestPossibleBoardName(targetwit, targetProcess);
            return new KeyValuePair<string, string>(key, value);
        }

        public string GetBestPossibleBoardName(string wit, string sourceProcess)
        {
            string boardName = null;

            if (wit.ToLower().Contains("epic"))
                boardName = "Epics";
            else if (wit.ToLower().Contains("feature"))
                boardName = "Features";
            else if (wit.ToLower().Contains("productbacklogitem"))
                boardName = "Backlog Items";
            else if (wit.ToLower().Contains("userstory"))
                boardName = "Stories";
            else if (wit.ToLower().Contains("requirement"))
                boardName = "Requirements";

            return boardName;
        }

        public void AddOrUpdateProcessMap(ProcessMap bestMap)
        {
            var current = this.GetProcessMap(bestMap.SourceProcess, bestMap.TargetProcess);
            if (current != null)
            {
                this.ProcessMaps.Remove(current);
                this.ProcessMaps.Add(bestMap);
            }
            else
            {
                this.ProcessMaps.Add(bestMap);
            }
        }

        public ProcessMap GetBestProcessMap(string sourceProcess, string targetProcess)
        {
            return this.ProcessMaps.SingleOrDefault(
                pm =>
                string.Equals(pm.SourceProcess, sourceProcess, StringComparison.OrdinalIgnoreCase)
                &&
                string.Equals(pm.TargetProcess, targetProcess, StringComparison.OrdinalIgnoreCase)
                );
        }

        public void SaveToJson(string outputFile)
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            FileInfo fi = new FileInfo(outputFile);
            if (!Directory.Exists(fi.Directory.FullName))
                Directory.CreateDirectory(fi.Directory.FullName);

            File.WriteAllText(fi.FullName, json);
        }

        public static Maps LoadFromJson(string inputFile)
        {
            string json = File.ReadAllText(inputFile);
            Maps maps = JsonConvert.DeserializeObject<Maps>(json);
            maps.LoadAllProcessesFromFolder(maps.InheritedProcessSourcePath);
            return maps;
        }

        public bool Validate()
        {
            bool isValid = true;
            foreach (var process in this.InheritedProcessDictionary)
            {
                if (process.Key != process.Value.Process.Name)
                {
                    isValid = false;
                    Trace.WriteLine($"process {process.Key} should match {process.Value.Process.Name}");
                    return isValid;
                }
            }
            foreach (var item in this.ProcessMaps)
            {
                isValid = isValid && item.Validate(this);
                if (!isValid)
                    return isValid;
            }
            return isValid;
        }
    }
    public class ProcessMap
    {
        public string SourceProcess { get; set; }
        public string TargetProcess { get; set; }

        public Dictionary<string, string> WorkItemTypeMap { get; set; }
        public HashSet<string> UnmappedSourceWorkItemTypes { get; set; }
        public Dictionary<string, Dictionary<string, string>> WorkItemTypeStateMap { get; set; }
        public Dictionary<string, HashSet<string>> UnmappedSourceWorkItemTypeStateMaps { get; set; }
        public Dictionary<string, Dictionary<string, string>> WorkItemTypeFieldMap { get; set; }
        public Dictionary<string, HashSet<string>> UnmappedSourceWorkItemTypeFieldMaps { get; set; }
        public Dictionary<string, KeyValuePair<string, string>> WorkItemTypeBoardMap { get; set; }
        public HashSet<string> UnmappedSourceWorkItemTypeBoardMaps { get; set; }

        public Dictionary<string, string> NonTrivialWorkItemTypeMap { get; set; }
        public Dictionary<string, Dictionary<string, string>> NonTrivialWorkItemTypeFieldMap { get; set; }
        public Dictionary<string, Dictionary<string, string>> NonTrivialWorkItemTypeStateMap { get; set; }
        public Dictionary<string, KeyValuePair<string, string>> NonTrivialWorkItemTypeBoardMap { get; set; }

        public bool HasUnmappedItems()
        {
            return this.TotalNumberOfUnmappedItems() > 0;
        }

        public int NumberOfUnmappedWorkItemTypes()
        {
            return UnmappedSourceWorkItemTypes.Count;
        }
        public int NumberOfUnmappedWorkItemTypeFields()
        {
            return this.UnmappedSourceWorkItemTypeFieldMaps.Sum(l => l.Value.Count);
        }
        public int NumberOfUnmappedWorkItemTypeStates()
        {
            return this.UnmappedSourceWorkItemTypeStateMaps.Sum(l => l.Value.Count);
        }
        public int NumberOfUnmappedWorkItemTypeBoards()
        {
            return this.UnmappedSourceWorkItemTypeBoardMaps.Count;
        }

        public int TotalNumberOfUnmappedItems()
        {
            return this.NumberOfUnmappedWorkItemTypes()
                + this.NumberOfUnmappedWorkItemTypeFields()
                + this.NumberOfUnmappedWorkItemTypeStates()
                + this.NumberOfUnmappedWorkItemTypeBoards()
                ;
        }



        public bool Validate(Maps maps)
        {
            InheritedProcess sourceProcess;
            InheritedProcess targetProcess;

            bool isValid = true;
            if (maps.InheritedProcessDictionary.ContainsKey(this.SourceProcess))
            {
                sourceProcess = maps.InheritedProcessDictionary[this.SourceProcess];
            }
            else
            {
                isValid = false;
                Trace.WriteLine($"can't find source process {this.SourceProcess}");
                return isValid;
            }

            if (maps.InheritedProcessDictionary.ContainsKey(this.TargetProcess))
            {
                targetProcess = maps.InheritedProcessDictionary[this.TargetProcess];
            }
            else
            {
                isValid = false;
                Trace.WriteLine($"can't find target process {this.TargetProcess}");
                return isValid;
            }

            //check work items
            if (this.WorkItemTypeMap != null)
            {
                foreach (var entry in this.WorkItemTypeMap)
                {
                    if (!sourceProcess.WorkItemTypes.Any(wit => wit.Id == entry.Key))
                    {
                        isValid = false;
                        Trace.WriteLine($"can't find wit {entry.Key} in source {this.SourceProcess}");
                        return isValid;
                    }
                    if (!targetProcess.WorkItemTypes.Any(wit => wit.Id == entry.Value))
                    {
                        isValid = false;
                        Trace.WriteLine($"can't find wit {entry.Value} in target {this.TargetProcess}");
                        return isValid;
                    }
                }
            }

            //check work item fields
            if (this.WorkItemTypeFieldMap != null)
            {
                foreach (var entry in this.WorkItemTypeFieldMap)
                {
                    //ensure wit is present
                    if (!sourceProcess.WorkItemTypes.Any(wit => wit.Id == entry.Key))
                    {
                        isValid = false;
                        Trace.WriteLine($"can't find wit {entry.Key} in source {this.SourceProcess}");
                        return isValid;
                    }

                    var sourceWitFields = sourceProcess.WorkItemTypeFields
                            .Single(bb => bb.WorkItemTypeRefName == entry.Key);
                    var targetWitFields = targetProcess.WorkItemTypeFields
                        .Single(bb => bb.WorkItemTypeRefName == this.WorkItemTypeMap[entry.Key]);

                    foreach (var insideEntry in entry.Value)
                    {
                        if (!sourceWitFields.Fields.Any(wit => wit.ReferenceName == insideEntry.Key))
                        {
                            isValid = false;
                            Trace.WriteLine($"can't find wit field {insideEntry.Key} for wit {entry.Key} in source {this.SourceProcess}");
                            return isValid;
                        }
                        if (!targetWitFields.Fields.Any(wit => wit.ReferenceName == insideEntry.Value))
                        {
                            isValid = false;
                            Trace.WriteLine($"can't find wit field {insideEntry.Value} for wit {entry.Key} in target {this.TargetProcess}");
                            return isValid;
                        }
                    }
                }
            }

            //check work item states
            if (this.WorkItemTypeStateMap != null)
            {
                foreach (var entry in this.WorkItemTypeStateMap)
                {
                    //ensure wit is present
                    if (!sourceProcess.WorkItemTypes.Any(wit => wit.Id == entry.Key))
                    {
                        isValid = false;
                        Trace.WriteLine($"can't find wit {entry.Key} in source {this.SourceProcess}");
                        return isValid;
                    }

                    var sourceWitStates = sourceProcess.States
                            .Single(bb => bb.WorkItemTypeRefName == entry.Key);
                    var targetWitStates = targetProcess.States
                        .Single(bb => bb.WorkItemTypeRefName == this.WorkItemTypeMap[entry.Key]);

                    foreach (var insideEntry in entry.Value)
                    {
                        if (!sourceWitStates.States.Any(wit => wit.Name == insideEntry.Key))
                        {
                            isValid = false;
                            Trace.WriteLine($"can't find wit State {insideEntry.Key} for wit {entry.Key} in source {this.SourceProcess}");
                            return isValid;
                        }
                        if (!targetWitStates.States.Any(wit => wit.Name == insideEntry.Value))
                        {
                            isValid = false;
                            Trace.WriteLine($"can't find wit State {insideEntry.Value} for wit {entry.Key} in target {this.TargetProcess}");
                            return isValid;
                        }
                    }
                }
            }

            //check unmapped
            if (this.UnmappedSourceWorkItemTypes != null)
            {
                foreach (var entry in this.UnmappedSourceWorkItemTypes)
                {
                    if (!sourceProcess.WorkItemTypes.Any(wit => wit.Id == entry))
                    {
                        isValid = false;
                        Trace.WriteLine($"can't find wit {entry} in source {this.SourceProcess}");
                        return isValid;
                    }
                    //make sure it's truly unmapped
                    if (this.WorkItemTypeMap.ContainsKey(entry))
                    {
                        isValid = false;
                        Trace.WriteLine($"wit {entry} is currently mapped - must remove from unmapped entries");
                        return isValid;
                    }
                }
            }
            if (this.UnmappedSourceWorkItemTypeFieldMaps != null)
            {
                foreach (var entry in this.UnmappedSourceWorkItemTypeFieldMaps)
                {
                    if (!sourceProcess.WorkItemTypes.Any(wit => wit.Id == entry.Key))
                    {
                        isValid = false;
                        Trace.WriteLine($"can't find wit {entry.Key} in source {this.SourceProcess}");
                        return isValid;
                    }
                    //make sure it's truly unmapped
                    foreach (var insideEntry in entry.Value)
                    {
                        if (!this.WorkItemTypeFieldMap.ContainsKey(entry.Key))
                        {
                            //should be ok since can't be mapped
                        }
                        else if (this.WorkItemTypeFieldMap[entry.Key].ContainsKey(insideEntry))
                        {
                            isValid = false;
                            Trace.WriteLine($"wit field {insideEntry} is currently mapped - must remove from unmapped entries");
                            return isValid;
                        }
                    }
                }
            }
            if (this.UnmappedSourceWorkItemTypeStateMaps != null)
            {
                foreach (var entry in this.UnmappedSourceWorkItemTypeStateMaps)
                {
                    if (!sourceProcess.WorkItemTypes.Any(wit => wit.Id == entry.Key))
                    {
                        isValid = false;
                        Trace.WriteLine($"can't find wit {entry.Key} in source {this.SourceProcess}");
                        return isValid;
                    }
                    //make sure it's truly unmapped
                    foreach (var insideEntry in entry.Value)
                    {
                        if (!this.WorkItemTypeStateMap.ContainsKey(entry.Key))
                        {
                            //should be ok since can't be mapped
                        }
                        else if (this.WorkItemTypeStateMap[entry.Key].ContainsKey(insideEntry))
                        {
                            isValid = false;
                            Trace.WriteLine($"wit state {insideEntry} is currently mapped - must remove from unmapped entries");
                            return isValid;
                        }
                    }
                }
            }

            return isValid;
        }
    }
}
