using ADO.RestAPI.Viewmodel50;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ADO.RestAPI.ProcessMapping
{
    public static class ProcessMappingUtility2
    {
        #region Private Helpers

        private static string MapAgileToCmmiState(string name)
        {
            if (name == null)
                return null;
            return name.Replace("New", "Proposed");
        }

        private static string MapScrumToCmmiState(string name)
        {
            if (name == null)
                return null;
            return name.Replace("New", "Proposed")
                .Replace("In Progress", "Active")
                .Replace("Done", "Closed")
                .Replace("Approved", "Active")
                .Replace("Committed", "Resolved")
                .Replace("Open", "Active") //for impediment to issue
                ;
        }

        private static string MapAgileToCmmiField(string fieldIdentifier)
        {
            return fieldIdentifier.Replace("Microsoft.VSTS.Scheduling.StoryPoints", "Microsoft.VSTS.Scheduling.Size");
        }

        private static string MapScrumToCmmiField(string fieldIdentifier)
        {
            return fieldIdentifier.Replace("Microsoft.VSTS.Scheduling.Effort", "Microsoft.VSTS.Scheduling.Size");
        }

        #endregion Private Helpers

        public static List<string> GetBoardTypes(string sourceProcessType,
            Maps maps)
        {
            List<string> boardTypes;
            // Based on process template, add stories or backlog items or requirements types.                    
            if (maps.GetParentProcess(sourceProcessType) == Constants.AgileTemplateType)
                boardTypes = new List<string> { "Epics", "Features", "Stories" };
            else if (maps.GetParentProcess(sourceProcessType) == Constants.ScrumTemplateType)
                boardTypes = new List<string> { "Epics", "Features", "Backlog Items" };
            else if (maps.GetParentProcess(sourceProcessType) == Constants.CmmiTemplateType)
                boardTypes = new List<string> { "Epics", "Features", "Requirements" };
            else
            {
                throw new NotImplementedException($"how to handle boards for {sourceProcessType}");
            }

            return boardTypes;
        }

        public static string MapBoardName(string sourceBoardName,
            string sourceProcessType,
            string destinationProcessType,
            Maps maps)
        {
            if (sourceProcessType.ToLower() == destinationProcessType.ToLower())
            {
                return sourceBoardName;
            }
            else
            {
                InheritedProcess sourceProcess = maps.InheritedProcessDictionary[sourceProcessType];
                InheritedProcess targetProcess = maps.InheritedProcessDictionary[destinationProcessType];

                ProcessMap processMap = maps.GetBestProcessMap(sourceProcessType, destinationProcessType);
                if (processMap != null)
                {
                    var isBoardFound = processMap.WorkItemTypeBoardMap.Any(bb => bb.Value.Key == sourceBoardName);
                    if (isBoardFound)
                    {
                        var boardFound = processMap.WorkItemTypeBoardMap.Single(bb => bb.Value.Key == sourceBoardName);
                        return boardFound.Value.Value;
                    }
                    else
                    {
                        throw new NotImplementedException($"map board name from process {sourceProcessType} to process {destinationProcessType} and board {sourceBoardName}");
                    }
                }
                else
                {
                    throw new NotImplementedException($"map board name from process {sourceProcessType} to process {destinationProcessType}");
                }
            }
        }

        private static string MapState(string sourceState,
            string sourceBoardName,
            string sourceProcessType,
            string destinationProcessType,
            Maps maps)
        {
            if (sourceProcessType.ToLower() == destinationProcessType.ToLower())
            {
                return sourceState;
            }
            else
            {
                InheritedProcess sourceProcess = maps.InheritedProcessDictionary[sourceProcessType];
                InheritedProcess targetProcess = maps.InheritedProcessDictionary[destinationProcessType];

                ProcessMap processMap = maps.GetBestProcessMap(sourceProcessType, destinationProcessType);
                if (processMap != null)
                {
                    var boardFound = processMap.WorkItemTypeBoardMap.Single(bb => bb.Value.Key == sourceBoardName);
                    var witFound = boardFound.Key;
                    if (processMap.WorkItemTypeStateMap.ContainsKey(witFound))
                    {
                        var stateFound = processMap.WorkItemTypeStateMap[witFound].Any(ss => ss.Key == sourceState);
                        if (stateFound)
                        {
                            return processMap.WorkItemTypeStateMap[witFound].Single(ss => ss.Key == sourceState).Value;
                        }
                        else
                        {
                            return sourceState;
                        }
                    }
                    else
                    {
                        return sourceState;
                    }                    
                }
                else
                {
                    throw new NotImplementedException($"map state from process {sourceProcessType} to process {destinationProcessType}");
                }
            }
        }

        public static string MapField(string sourceField,
            string sourceBoardName,
            string sourceProcessType,
            string destinationProcessType,
            Maps maps)
        {
            if (sourceProcessType.ToLower() == destinationProcessType.ToLower())
            {
                return sourceField;
            }
            else
            {
                InheritedProcess sourceProcess = maps.InheritedProcessDictionary[sourceProcessType];
                InheritedProcess targetProcess = maps.InheritedProcessDictionary[destinationProcessType];

                ProcessMap processMap = maps.GetBestProcessMap(sourceProcessType, destinationProcessType);
                if (processMap != null)
                {
                    var boardFound = processMap.WorkItemTypeBoardMap.Single(bb => bb.Value.Key == sourceBoardName);
                    var witFound = boardFound.Key;
                    if (processMap.WorkItemTypeFieldMap.ContainsKey(witFound))
                    {
                        var fieldFound = processMap.WorkItemTypeFieldMap[witFound].Any(ss => ss.Key == sourceField);
                        if (fieldFound)
                        {
                            return processMap.WorkItemTypeFieldMap[witFound].Single(ss => ss.Key == sourceField).Value;
                        }
                        else
                        {
                            return sourceField;
                        }
                    }
                    else
                    {
                        return sourceField;
                    }
                }
                else
                {
                    throw new NotImplementedException($"map field from process {sourceProcessType} to process {destinationProcessType}");
                }
            }
        }

        public static string InsertMissingCmmiBoardColumnsFromScrum(string boardName, string state, string expectedBoardName)
        {
            if (boardName == expectedBoardName)
                return state;
            else
                return null;
        }

        public static WorkResponse.StateMappings MapStateMappings(
            WorkResponse.StateMappings sourceStateMappings, 
            string sourceBoardName, 
            string sourceProcessType, 
            string destinationProcessType, 
            Maps maps)
        {
            if (sourceProcessType.ToLower() == destinationProcessType.ToLower())
            {
                return sourceStateMappings;
            }
            else
            {
                InheritedProcess sourceProcess = maps.InheritedProcessDictionary[sourceProcessType];
                InheritedProcess targetProcess = maps.InheritedProcessDictionary[destinationProcessType];

                ProcessMap processMap = maps.GetBestProcessMap(sourceProcessType, destinationProcessType);
                if (processMap != null)
                {
                    var boardFound = processMap.WorkItemTypeBoardMap.Single(bb => bb.Value.Key == sourceBoardName);
                    var witFound = boardFound.Key;
                    var targetBoardName = boardFound.Value.Value;
                    if (processMap.WorkItemTypeStateMap.ContainsKey(witFound))
                    {
                        //var stateMap = processMap.WorkItemTypeStateMap[witFound];
                        if (sourceBoardName == targetBoardName)
                        {
                            string oldValue = sourceStateMappings.GetValue(sourceBoardName);
                            string newValue = MapState(oldValue, sourceBoardName, sourceProcessType, destinationProcessType, maps);
                            sourceStateMappings.SetValue(sourceBoardName, newValue);
                            //check for bug
                            if (!string.IsNullOrEmpty(sourceStateMappings.Bug))
                            {
                                //throw new NotImplementedException("may need to implement this for bugs");
                                //careful with hardcoded values
                                string oldBugValue = sourceStateMappings.GetValue(Constants.Bugs);
                                var bugStateMap = processMap.WorkItemTypeStateMap.Single(it => it.Key.EndsWith($".{Constants.Bug}"));
                                string newBugValue = oldBugValue;
                                if (bugStateMap.Value.ContainsKey(oldBugValue))
                                    newBugValue = bugStateMap.Value[oldBugValue];
                                else
                                {
                                    throw new NotImplementedException($"map BUG {bugStateMap.Key} state {oldBugValue} from process {sourceProcessType} to process {destinationProcessType} NOT found");
                                }
                                sourceStateMappings.SetValue(Constants.Bugs, newBugValue);
                            }
                            return sourceStateMappings;
                        }
                        else
                        {
                            string oldValue = sourceStateMappings.GetValue(sourceBoardName);
                            //set old value to null
                            sourceStateMappings.SetValue(sourceBoardName, null);
                            string newValue = MapState(oldValue, sourceBoardName, sourceProcessType, destinationProcessType, maps);
                            sourceStateMappings.SetValue(targetBoardName, newValue);
                            //check for bug
                            if (!string.IsNullOrEmpty(sourceStateMappings.Bug))
                            {
                                //careful with hardcoded values
                                string oldBugValue = sourceStateMappings.GetValue(Constants.Bugs);                                
                                var bugStateMap = processMap.WorkItemTypeStateMap.Single(it=>it.Key.EndsWith($".{Constants.Bug}"));
                                string newBugValue = oldBugValue;
                                if (bugStateMap.Value.ContainsKey(oldBugValue))
                                    newBugValue = bugStateMap.Value[oldBugValue];
                                else
                                {
                                    throw new NotImplementedException($"map BUG {bugStateMap.Key} state {oldBugValue} from process {sourceProcessType} to process {destinationProcessType} NOT found");
                                }
                                sourceStateMappings.SetValue(Constants.Bugs, newBugValue);
                            }
                            return sourceStateMappings;
                        }
                    }
                    else
                    {
                        return sourceStateMappings;
                    }
                }
                else
                {
                    throw new NotImplementedException($"map state from process {sourceProcessType} to process {destinationProcessType}");
                }
            }
        }
    }
}
