using TreeCollections;

namespace ADO.Engine.BusinessEntities
{
    public class ClassificationNodeEnhancedDataNode
    : SerialTreeNode<ClassificationNodeEnhancedDataNode>
    {
        // empty constructor is a type constraint imposed by the base class
        public ClassificationNodeEnhancedDataNode() { }

        public ClassificationNodeEnhancedDataNode(
            int id,
            string name,
            string structureType,
            bool hasChildren,
            string path,
            TeamLevel teamLevel,
            Attributes attributes,
            params ClassificationNodeEnhancedDataNode[] children)
            : base(children)
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
}
