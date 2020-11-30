using ADO.Engine.Utilities;
using System;
using TreeCollections;

namespace ADO.Engine.BusinessEntities
{
    public class BusinessNodeEntityDefinition<TValue> : NamedEntityDefinition<int, TValue>
        where TValue : BusinessNodeItem
    {
        public static readonly BusinessNodeEntityDefinition<TValue> Instance = new BusinessNodeEntityDefinition<TValue>();

        private BusinessNodeEntityDefinition() : base(
            c => c.Id, c => c.Name, StringComparer.OrdinalIgnoreCase)
        { }
    }
    public partial class SimpleMutableBusinessNodeNode
        : MutableEntityTreeNode<SimpleMutableBusinessNodeNode, int, string, BusinessNodeItem>
    {
        public SimpleMutableBusinessNodeNode(BusinessNodeItem item)
            : base(BusinessNodeEntityDefinition<BusinessNodeItem>.Instance, item, ErrorCheckOptions.All)
        { }

        private SimpleMutableBusinessNodeNode(BusinessNodeItem item, SimpleMutableBusinessNodeNode parent)
            : base(item, parent)
        { }

        protected override SimpleMutableBusinessNodeNode Create(BusinessNodeItem item, SimpleMutableBusinessNodeNode parent)
        {
            return new SimpleMutableBusinessNodeNode(item, parent);
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
    public class ReadOnlyBusinessNodeNode
        : ReadOnlyEntityTreeNode<ReadOnlyBusinessNodeNode, int, DualStateBusinessNodeItem>
    {
        public ReadOnlyBusinessNodeNode(DualStateBusinessNodeItem item)
            : base(BusinessNodeEntityDefinition<DualStateBusinessNodeItem>.Instance, item, ErrorCheckOptions.All)
        {
            HierarchyCount = 1;
        }

        private ReadOnlyBusinessNodeNode(DualStateBusinessNodeItem item, ReadOnlyBusinessNodeNode parent)
            : base(item, parent)
        { }

        protected override ReadOnlyBusinessNodeNode Create(DualStateBusinessNodeItem item, ReadOnlyBusinessNodeNode parent)
        {
            return new ReadOnlyBusinessNodeNode(item, parent);
        }

        public int HierarchyCount { get; private set; }

        protected override void OnNodeAttached()
        {
            SelectPathUpward().ForEach(n => n.HierarchyCount++);
        }
    }
}
