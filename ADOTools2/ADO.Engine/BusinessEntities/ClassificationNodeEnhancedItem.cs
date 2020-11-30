namespace ADO.Engine.BusinessEntities
{
    public class ClassificationNodeEnhancedItem
    {
        public ClassificationNodeEnhancedItem(
            int id,
            string name,
            string structureType,
            bool hasChildren,
            string path,
            TeamLevel teamLevel,
            Attributes attributes)
        {
            Id = id;
            Name = name;
            StructureType = structureType;
            HasChildren = hasChildren;
            Path = path;
            TeamLevel = teamLevel;
            Attributes = attributes;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string StructureType { get; set; }
        public bool HasChildren { get; set; }
        public string Path { get; set; }
        public TeamLevel TeamLevel { get; set; }
        public Attributes Attributes { get; set; }
    }

    public class DualStateClassificationNodeMinimalItem : ClassificationNodeEnhancedItem
    {
        public DualStateClassificationNodeMinimalItem(
            int id,
            string name,
            string structureType,
            bool hasChildren,
            string path,
            TeamLevel teamLevel,
            Attributes attributes
            )
            : base(
                  id,
                  name,
                  structureType,
                  hasChildren,
                  path,
                  teamLevel,
                  attributes)
        {
            IsEnabled = true;
        }

        public DualStateClassificationNodeMinimalItem(ClassificationNodeEnhancedItem sourceItem)
            : base(
                  sourceItem.Id,
                  sourceItem.Name,
                  sourceItem.StructureType,
                  sourceItem.HasChildren,
                  sourceItem.Path,
                  sourceItem.TeamLevel,
                  sourceItem.Attributes)
        {
            IsEnabled = true;
        }

        public bool IsEnabled { get; set; }
    }
}
