using System;
using System.Linq;

namespace ADO.Engine.BusinessEntities
{
    public class ClassificationNodeMinimalWithIdItem
    {
        public ClassificationNodeMinimalWithIdItem(
            int id,
            string name,
            string structureType,
            bool hasChildren,
            string path,
            Attributes attributes)
        {
            Id = id;
            Name = name;
            StructureType = structureType;
            HasChildren = hasChildren;
            Path = path;
            Attributes = attributes;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string StructureType { get; set; }
        public bool HasChildren { get; set; }
        public string Path { get; set; }
        public Attributes Attributes { get; set; }

        public string GetClassificationNodePathWithoutRootAndStructureNodes(string pathSeparatorToUse)
        {
            var pathParts = this.Path.Split(
                new string[] { Constants.DefaultPathSeparator, Constants.DefaultPathSeparatorForward }, 
                StringSplitOptions.RemoveEmptyEntries);
            if (pathParts.Count() < 2)
                throw new ArgumentException("expecting at least 2 nodes in path");
            if (pathParts.Count() == 2)
                return string.Empty;
            else
                return String.Join(pathSeparatorToUse, pathParts.Skip(2));
        }
    }

    public class DualStateClassificationNodeMinimalWithIdItem : ClassificationNodeMinimalWithIdItem
    {
        public DualStateClassificationNodeMinimalWithIdItem(
            int id,
            string name,
            string structureType,
            bool hasChildren,
            string path,
            Attributes attributes
            )
            : base(
                  id,
                  name,
                  structureType,
                  hasChildren,
                  path,
                  attributes)
        {
            IsEnabled = true;
        }

        public DualStateClassificationNodeMinimalWithIdItem(ClassificationNodeMinimalWithIdItem sourceItem)
            : base(
                  sourceItem.Id,
                  sourceItem.Name,
                  sourceItem.StructureType,
                  sourceItem.HasChildren,
                  sourceItem.Path,
                  sourceItem.Attributes)
        {
            IsEnabled = true;
        }

        public bool IsEnabled { get; set; }
    }
}
