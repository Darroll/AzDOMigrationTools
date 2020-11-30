using TreeCollections;

namespace ADO.Engine.BusinessEntities
{
    public class ClassificationNodeMinimalWithIdDataNode
    : SerialTreeNode<ClassificationNodeMinimalWithIdDataNode>
    {
        // empty constructor is a type constraint imposed by the base class
        public ClassificationNodeMinimalWithIdDataNode() { }

        public ClassificationNodeMinimalWithIdDataNode(
            int id,
            string name,
            string structureType,
            bool hasChildren,
            string path,
            Attributes attributes,
            params ClassificationNodeMinimalWithIdDataNode[] children)
            : base(children)
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
    }
}
