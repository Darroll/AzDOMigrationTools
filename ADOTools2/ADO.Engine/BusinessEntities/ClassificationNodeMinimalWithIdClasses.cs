using ADO.Engine.Utilities;
using System;
using TreeCollections;

namespace ADO.Engine.BusinessEntities
{
    public class ClassificationNodeMinimalWithIdEntityDefinition<TValue> : NamedEntityDefinition<int, TValue>
        where TValue : ClassificationNodeMinimalWithIdItem
    {
        public static readonly ClassificationNodeMinimalWithIdEntityDefinition<TValue> Instance = new ClassificationNodeMinimalWithIdEntityDefinition<TValue>();

        private ClassificationNodeMinimalWithIdEntityDefinition() : base(
            c => c.Id, c => c.Name, StringComparer.OrdinalIgnoreCase)
        { }
    }
    public partial class SimpleMutableClassificationNodeMinimalWithIdNode
        : MutableEntityTreeNode<SimpleMutableClassificationNodeMinimalWithIdNode, int, string, ClassificationNodeMinimalWithIdItem>
    {
        public SimpleMutableClassificationNodeMinimalWithIdNode(ClassificationNodeMinimalWithIdItem item)
            : base(ClassificationNodeMinimalWithIdEntityDefinition<ClassificationNodeMinimalWithIdItem>.Instance, item, ErrorCheckOptions.All)
        { }

        private SimpleMutableClassificationNodeMinimalWithIdNode(ClassificationNodeMinimalWithIdItem item, SimpleMutableClassificationNodeMinimalWithIdNode parent)
            : base(item, parent)
        { }

        protected override SimpleMutableClassificationNodeMinimalWithIdNode Create(ClassificationNodeMinimalWithIdItem item, SimpleMutableClassificationNodeMinimalWithIdNode parent)
        {
            return new SimpleMutableClassificationNodeMinimalWithIdNode(item, parent);
        }

        protected override void SetItemName(string name)
        {
            Item.Name = name;
        }

        protected override bool ContinueAddOnExistingError()
        {
            Console.WriteLine($"Cancelled adding to {Id} {Item.Name.WrapDoubleQuotes()} due to error: {Error}");

            return false;
        }        
    }
    public class ReadOnlyClassificationNodeMinimalWithIdNode
        : ReadOnlyEntityTreeNode<ReadOnlyClassificationNodeMinimalWithIdNode, int, DualStateClassificationNodeMinimalWithIdItem>
    {
        public ReadOnlyClassificationNodeMinimalWithIdNode(DualStateClassificationNodeMinimalWithIdItem item)
            : base(ClassificationNodeMinimalWithIdEntityDefinition<DualStateClassificationNodeMinimalWithIdItem>.Instance, item, ErrorCheckOptions.All)
        {
            HierarchyCount = 1;
        }

        private ReadOnlyClassificationNodeMinimalWithIdNode(DualStateClassificationNodeMinimalWithIdItem item, ReadOnlyClassificationNodeMinimalWithIdNode parent)
            : base(item, parent)
        { }

        protected override ReadOnlyClassificationNodeMinimalWithIdNode Create(DualStateClassificationNodeMinimalWithIdItem item, ReadOnlyClassificationNodeMinimalWithIdNode parent)
        {
            return new ReadOnlyClassificationNodeMinimalWithIdNode(item, parent);
        }

        public int HierarchyCount { get; private set; }

        protected override void OnNodeAttached()
        {
            SelectPathUpward().ForEach(n => n.HierarchyCount++);
        }
    }
}
