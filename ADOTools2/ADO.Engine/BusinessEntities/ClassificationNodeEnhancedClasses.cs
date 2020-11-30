using ADO.Engine.Utilities;
using System;
using TreeCollections;

namespace ADO.Engine.BusinessEntities
{
    public class ClassificationNodeEnhancedEntityDefinition<TValue> : NamedEntityDefinition<int, TValue>
        where TValue : ClassificationNodeEnhancedItem
    {
        public static readonly ClassificationNodeEnhancedEntityDefinition<TValue> Instance = new ClassificationNodeEnhancedEntityDefinition<TValue>();

        private ClassificationNodeEnhancedEntityDefinition() : base(
            c => c.Id, c => c.Name, StringComparer.OrdinalIgnoreCase)
        { }
    }
    public partial class SimpleMutableClassificationNodeEnhancedNode
        : MutableEntityTreeNode<SimpleMutableClassificationNodeEnhancedNode, int, string, ClassificationNodeEnhancedItem>
    {
        public SimpleMutableClassificationNodeEnhancedNode(ClassificationNodeEnhancedItem item)
            : base(ClassificationNodeEnhancedEntityDefinition<ClassificationNodeEnhancedItem>.Instance, item, ErrorCheckOptions.All)
        { }

        private SimpleMutableClassificationNodeEnhancedNode(ClassificationNodeEnhancedItem item, SimpleMutableClassificationNodeEnhancedNode parent)
            : base(item, parent)
        { }

        protected override SimpleMutableClassificationNodeEnhancedNode Create(ClassificationNodeEnhancedItem item, SimpleMutableClassificationNodeEnhancedNode parent)
        {
            return new SimpleMutableClassificationNodeEnhancedNode(item, parent);
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
    public class ReadOnlyClassificationNodeEnhancedNode
        : ReadOnlyEntityTreeNode<ReadOnlyClassificationNodeEnhancedNode, int, DualStateClassificationNodeMinimalItem>
    {
        public ReadOnlyClassificationNodeEnhancedNode(DualStateClassificationNodeMinimalItem item)
            : base(ClassificationNodeEnhancedEntityDefinition<DualStateClassificationNodeMinimalItem>.Instance, item, ErrorCheckOptions.All)
        {
            HierarchyCount = 1;
        }

        private ReadOnlyClassificationNodeEnhancedNode(DualStateClassificationNodeMinimalItem item, ReadOnlyClassificationNodeEnhancedNode parent)
            : base(item, parent)
        { }

        protected override ReadOnlyClassificationNodeEnhancedNode Create(DualStateClassificationNodeMinimalItem item, ReadOnlyClassificationNodeEnhancedNode parent)
        {
            return new ReadOnlyClassificationNodeEnhancedNode(item, parent);
        }

        public int HierarchyCount { get; private set; }

        protected override void OnNodeAttached()
        {
            SelectPathUpward().ForEach(n => n.HierarchyCount++);
        }
    }
}
