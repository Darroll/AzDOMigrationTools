using ADO.Engine.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ADO.Engine.BusinessEntities
{
    public class ClassificationNodeMinimalWithId
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

        [JsonProperty(PropertyName = "attributes")]
        public Attributes Attributes { get; set; }

        [JsonProperty(PropertyName = "children")]
        public List<ClassificationNodeMinimalWithId> Children { get; set; }

        public static ClassificationNodeMinimalWithId LoadFromJson(string finalAreaHierarchyPath)
        {
            return finalAreaHierarchyPath.LoadFromJson<ClassificationNodeMinimalWithId>();
        }

        public void RenameRoot(string newClassificationRootName)
        {
            RenameRootRecursive(newClassificationRootName, true);
        }
        private void RenameRootRecursive(string newClassificationRootName, bool isRoot)
        {
            if (isRoot)
            {
                this.Name = newClassificationRootName;
            }
            this.Path = ClassificationNodeMinimal.RenamePath(this.Path, newClassificationRootName);
            if (this.Children != null)
            {
                foreach (var child in this.Children)
                {
                    child.RenameRootRecursive(newClassificationRootName, false);
                }
            }
        }

        public static string RenamePath(string path, string newClassificationRootName)
        {
            var pathParts = path.Split(new string[] { Constants.DefaultPathSeparator }, StringSplitOptions.RemoveEmptyEntries);
            pathParts[0] = newClassificationRootName;
            string retPath = $"{Constants.DefaultPathSeparator}{String.Join(Constants.DefaultPathSeparator, pathParts)}";
            return retPath;
        }

    }
}
