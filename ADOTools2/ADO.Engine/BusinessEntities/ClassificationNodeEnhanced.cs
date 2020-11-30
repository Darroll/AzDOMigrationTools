using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TreeCollections;

namespace ADO.Engine.BusinessEntities
{
    public class ClassificationNodeEnhanced
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "structureType")]
        public string StructureType { get; set; }

        [JsonProperty(PropertyName = "hasChildren")]
        public bool HasChildren { get; set; }

        [JsonProperty(PropertyName = "path")]
        public string Path { get; set; }

        [JsonProperty(PropertyName = "teamLevel")]
        public TeamLevel TeamLevel { get; set; }

        [JsonProperty(PropertyName = "attributes")]
        public Attributes Attributes { get; set; }

        [JsonProperty(PropertyName = "children")]
        public List<ClassificationNodeEnhanced> Children { get; set; }

        public SimpleMutableClassificationNodeEnhancedNode ToSimpleMutableClassificationNodeEnhancedNode(
            out Dictionary<int, SimpleMutableClassificationNodeEnhancedNode> mapFromClassNodeIdsToTreeNodes)
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            ClassificationNodeEnhancedDataNode dataRoot
                = JsonConvert.DeserializeObject<ClassificationNodeEnhancedDataNode>(json);

            var root = new SimpleMutableClassificationNodeEnhancedNode(
                new ClassificationNodeEnhancedItem(
                    dataRoot.Id,
                    dataRoot.Name,
                    dataRoot.StructureType,
                    dataRoot.HasChildren,
                    dataRoot.Path,
                    dataRoot.TeamLevel,
                    dataRoot.Attributes
                    ));
            root.Build(dataRoot, n =>
            new ClassificationNodeEnhancedItem(
                n.Id,
                n.Name,
                n.StructureType,
                n.HasChildren,
                n.Path,
                n.TeamLevel,
                n.Attributes
                ));
            mapFromClassNodeIdsToTreeNodes =
                root.Select(tn => tn).ToDictionary(k => k.Item.Id, v => v);
            return root;
        }
    }

    public class Attributes
    {
        [JsonProperty(PropertyName = "startDate")]
        public DateTime StartDate { get; set; }

        [JsonProperty(PropertyName = "finishDate")]
        public DateTime FinishDate { get; set; }
    }

}