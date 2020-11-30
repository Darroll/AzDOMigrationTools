using System;

namespace ADO.Engine.BusinessEntities
{
    public partial class SimpleMutableClassificationNodeEnhancedNode
    {
        public bool ContainsDate(DateTime dateTime)
        {
            return dateTime.Date >= this.Item.Attributes.StartDate && dateTime.Date <= this.Item.Attributes.FinishDate;
        }
    }
}
