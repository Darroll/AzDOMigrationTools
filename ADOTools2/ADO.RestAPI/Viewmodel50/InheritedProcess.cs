using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ADO.RestAPI.Viewmodel50
{
    #region Helper Classes
    public class WorkItemTypes
    {
        public int count { get; set; }
        public List<WorkItemType> value { get; set; }
    }
    public class WorkItemTypeStates
    {
        public int count { get; set; }
        public List<State1> value { get; set; }
    }
    public class WorkItemTypeFields
    {
        public int count { get; set; }
        public List<Field1> value { get; set; }
    }
    public class WorkItemTypeRules
    {
        public int count { get; set; }
        public List<Rule1> value { get; set; }
    }
    public class Picklists
    {
        public int count { get; set; }
        public List<PicklistValue> value { get; set; }
    }

    public class PicklistValue
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public bool isSuggested { get; set; }
        public string url { get; set; }
    }
    public class Picklist2
    {
        public List<string> items { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public bool isSuggested { get; set; }
        public string url { get; set; }
    }

    #endregion Helper Classes

    public class InheritedProcess
    {
        [JsonProperty(PropertyName = "process")]
        public ProcessSummary Process { get; set; }

        [JsonProperty(PropertyName = "fields")]
        public List<Field> Fields { get; set; }

        [JsonProperty(PropertyName = "workItemTypeFields")]
        public List<WorkItemTypeField> WorkItemTypeFields { get; set; }

        [JsonProperty(PropertyName = "workItemTypes")]
        public List<WorkItemType> WorkItemTypes { get; set; }

        [JsonProperty(PropertyName = "layouts")]
        public List<ProcessLayout> Layouts { get; set; }

        [JsonProperty(PropertyName = "states")]
        public List<State> States { get; set; }

        [JsonProperty(PropertyName = "rules")]
        public List<Rule> Rules { get; set; }

        [JsonProperty(PropertyName = "behaviors")]
        public List<Behavior> Behaviors { get; set; }

        [JsonProperty(PropertyName = "workItemTypeBehaviors")]
        public List<WorkItemTypeBehavior> WorkItemTypeBehaviors { get; set; }

        [JsonProperty(PropertyName = "witFieldPicklists")]
        public List<WitFieldPicklist> WitFieldPicklists { get; set; }

        public static InheritedProcess LoadFromJson(string path)
        {
            string value = File.ReadAllText(path);
            InheritedProcess newInheritedProcess = JsonConvert.DeserializeObject<InheritedProcess>(value);
            return newInheritedProcess;
        }

        public static void MergeParentIntoChild(InheritedProcess parent, InheritedProcess child)
        {
            foreach (var parentWit in parent.WorkItemTypes)
            {
                if (child.WorkItemTypes.Any(wit => wit.Name == parentWit.Name))
                {
                    var childWit = child.WorkItemTypes.Single(wit => wit.Name == parentWit.Name);
                    var parentFields = parent.WorkItemTypeFields.Single(f => f.WorkItemTypeRefName == parentWit.Id).Fields;
                    var childFields = child.WorkItemTypeFields.Single(f => f.WorkItemTypeRefName == childWit.Id).Fields;
                    foreach (var parentField in parentFields)
                    {
                        if (!childFields.Any(cf => cf.ReferenceName == parentField.ReferenceName))
                        {
                            childFields.Add(parentField);
                        }
                        else
                        {
                            var childField = childFields.Single(cf => cf.ReferenceName == parentField.ReferenceName);
                            System.Diagnostics.Trace.WriteLine($"child field {childField.ReferenceName} already exists");
                        }
                    }
                    //do also states
                    var parentStates = parent.States.Single(f => f.WorkItemTypeRefName == parentWit.Id).States;
                    var childStates = child.States.Single(f => f.WorkItemTypeRefName == childWit.Id).States;
                    foreach (var parentState in parentStates)
                    {
                        if (!childStates.Any(cf => cf.Name == parentState.Name))
                        {
                            childStates.Add(parentState);
                        }
                        else
                        {
                            var childState = childStates.Single(cf => cf.Name == parentState.Name);
                            System.Diagnostics.Trace.WriteLine($"child state {childState.Name} already exists");
                        }
                    }
                }
                else
                {
                    //doesn't exist
                    child.WorkItemTypes.Add(parentWit);
                    var parentFieldsItem = parent.WorkItemTypeFields.Single(f => f.WorkItemTypeRefName == parentWit.Id);
                    child.WorkItemTypeFields.Add(parentFieldsItem);
                    //do also states
                    var parentStatesItem = parent.States.Single(s => s.WorkItemTypeRefName == parentWit.Id);
                    child.States.Add(parentStatesItem);
                }
            }
        }

        public void SaveToJson(string path)
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(path, json);
        }
    }

    public class ProcessSummary
    {
        [JsonProperty(PropertyName = "typeId")]
        public string TypeId { get; set; }

        [JsonProperty(PropertyName = "referenceName")]
        public string ReferenceName { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "properties")]
        public Properties Properties { get; set; }
    }

    public class Properties
    {
        [JsonProperty(PropertyName = "class")]
        public int Class { get; set; }

        [JsonProperty(PropertyName = "parentProcessTypeId")]
        public string ParentProcessTypeId { get; set; }

        [JsonProperty(PropertyName = "isEnabled")]
        public bool IsEnabled { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "isDefault")]
        public bool IsDefault { get; set; }
    }

    public class Field
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "type")]
        public int Type { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "isIdentity")]
        public bool IsIdentity { get; set; }
    }

    public class WorkItemTypeField
    {
        [JsonProperty(PropertyName = "workItemTypeRefName")]
        public string WorkItemTypeRefName { get; set; }

        [JsonProperty(PropertyName = "fields")]
        public List<Field1> Fields { get; set; }
    }

    public class Field1
    {
        [JsonProperty(PropertyName = "referenceName")]
        public string ReferenceName { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "pickList")]
        public Picklist PickList { get; set; }

        [JsonProperty(PropertyName = "readOnly")]
        public bool ReadOnly { get; set; }

        [JsonProperty(PropertyName = "required")]
        public bool Required { get; set; }

        [JsonProperty(PropertyName = "defaultValue")]
        public string DefaultValue { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "allowGroups")]
        public bool? AllowGroups { get; set; }

        [JsonProperty(PropertyName = "customization")]
        public string Customization { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string description { get; set; }
    }

    public class Picklist
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "isSuggested")]
        public bool IsSuggested { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }

    public class WorkItemType
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "inherits")]
        public string Inherits { get; set; }

        [JsonProperty(PropertyName = "class")]
        public int Class { get; set; }

        [JsonProperty(PropertyName = "color")]
        public string Color { get; set; }

        [JsonProperty(PropertyName = "icon")]
        public string Icon { get; set; }

        [JsonProperty(PropertyName = "isDisabled")]
        public bool IsDisabled { get; set; }
    }

    public class ProcessLayout
    {
        [JsonProperty(PropertyName = "workItemTypeRefName")]
        public string WorkItemTypeRefName { get; set; }

        [JsonProperty(PropertyName = "layout")]
        public Layout1 Layout { get; set; }
    }

    public class Layout1
    {
        [JsonProperty(PropertyName = "pages")]
        public List<Page> pages { get; set; }

        [JsonProperty(PropertyName = "systemControls")]
        public List<SystemControl> SystemControls { get; set; }
    }

    public class Page
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "inherited")]
        public bool Inherited { get; set; }

        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        [JsonProperty(PropertyName = "pageType")]
        public int PageType { get; set; }

        [JsonProperty(PropertyName = "locked")]
        public bool Locked { get; set; }

        [JsonProperty(PropertyName = "visible")]
        public bool Visible { get; set; }

        [JsonProperty(PropertyName = "isContribution")]
        public bool IsContribution { get; set; }

        [JsonProperty(PropertyName = "sections")]
        public List<Section> Sections { get; set; }
    }

    public class Section
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "groups")]
        public List<Group> Groups { get; set; }
    }

    public class Group
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "inherited")]
        public bool Inherited { get; set; }

        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        [JsonProperty(PropertyName = "isContribution")]
        public bool IsContribution { get; set; }

        [JsonProperty(PropertyName = "visible")]
        public bool Visible { get; set; }

        [JsonProperty(PropertyName = "controls")]
        public List<Control> Controls { get; set; }
    }

    public class Control
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "inherited")]
        public bool Inherited { get; set; }

        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        [JsonProperty(PropertyName = "controlType")]
        public string ControlType { get; set; }

        [JsonProperty(PropertyName = "readOnly")]
        public bool ReadOnly { get; set; }

        [JsonProperty(PropertyName = "watermark")]
        public string Watermark { get; set; }

        [JsonProperty(PropertyName = "metadata")]
        public string Metadata { get; set; }

        [JsonProperty(PropertyName = "visible")]
        public bool Visible { get; set; }

        [JsonProperty(PropertyName = "isContribution")]
        public bool IsContribution { get; set; }

        [JsonProperty(PropertyName = "overridden")]
        public bool Overridden { get; set; }
    }

    public class SystemControl
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        [JsonProperty(PropertyName = "controlType")]
        public string ControlType { get; set; }

        [JsonProperty(PropertyName = "readOnly")]
        public bool ReadOnly { get; set; }

        [JsonProperty(PropertyName = "watermark")]
        public string Watermark { get; set; }

        [JsonProperty(PropertyName = "visible")]
        public bool Visible { get; set; }

        [JsonProperty(PropertyName = "isContribution")]
        public bool IsContribution { get; set; }
    }

    public class State
    {
        [JsonProperty(PropertyName = "workItemTypeRefName")]
        public string WorkItemTypeRefName { get; set; }

        [JsonProperty(PropertyName = "states")]
        public List<State1> States { get; set; }
    }

    public class State1
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "color")]
        public string Color { get; set; }

        [JsonProperty(PropertyName = "stateCategory")]
        public string StateCategory { get; set; }

        [JsonProperty(PropertyName = "order")]
        public int Order { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "hidden")]
        public bool Hidden { get; set; }
    }

    public class Rule
    {
        [JsonProperty(PropertyName = "workItemTypeRefName")]
        public string WorkItemTypeRefName { get; set; }

        [JsonProperty(PropertyName = "rules")]
        public List<Rule1> Rules { get; set; }
    }

    public class Rule1
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "friendlyName")]
        public object FriendlyName { get; set; }

        [JsonProperty(PropertyName = "conditions")]
        public List<Condition> Conditions { get; set; }

        [JsonProperty(PropertyName = "actions")]
        public List<Action> Actions { get; set; }

        [JsonProperty(PropertyName = "isDisabled")]
        public bool IsDisabled { get; set; }

        [JsonProperty(PropertyName = "isSystem")]
        public bool IsSystem { get; set; }
    }

    public class Condition
    {
        [JsonProperty(PropertyName = "conditionType")]
        public string ConditionType { get; set; }

        [JsonProperty(PropertyName = "field")]
        public string Field { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }

    public class Action
    {
        [JsonProperty(PropertyName = "actionType")]
        public string ActionType { get; set; }

        [JsonProperty(PropertyName = "targetField")]
        public string TargetField { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }

    public class Behavior
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "abstract")]
        public bool Abstract { get; set; }

        [JsonProperty(PropertyName = "overriden")]
        public bool Overriden { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "color")]
        public string Color { get; set; }

        [JsonProperty(PropertyName = "inherits")]
        public Inherits Inherits { get; set; }

        [JsonProperty(PropertyName = "rank")]
        public int Rank { get; set; }
    }

    public class Inherits
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }

    public class WorkItemTypeBehavior
    {
        [JsonProperty(PropertyName = "workItemType")]
        public WorkItemType1 WorkItemType { get; set; }

        [JsonProperty(PropertyName = "behaviors")]
        public List<Behavior1> Behaviors { get; set; }
    }

    public class WorkItemType1
    {
        [JsonProperty(PropertyName = "refName")]
        public string RefName { get; set; }

        [JsonProperty(PropertyName = "workItemTypeClass")]
        public int WorkItemTypeClass { get; set; }
    }

    public class Behavior1
    {
        [JsonProperty(PropertyName = "behavior")]
        public Behavior2 Behavior { get; set; }

        [JsonProperty(PropertyName = "isDefault")]
        public bool IsDefault { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }

    public class Behavior2
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }

    public class WitFieldPicklist
    {
        [JsonProperty(PropertyName = "workitemtypeRefName")]
        public string WorkitemtypeRefName { get; set; }

        [JsonProperty(PropertyName = "fieldRefName")]
        public string FieldRefName { get; set; }

        [JsonProperty(PropertyName = "picklist")]
        public Picklist1 Picklist { get; set; }
    }

    public class Picklist1
    {
        [JsonProperty(PropertyName = "items")]
        public List<Item> Items { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "isSuggested")]
        public bool IsSuggested { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }

    public class Item
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }















}
