using System;
using Newtonsoft.Json.Linq;

namespace VstsSyncMigrator.Engine.Configuration.FieldMap
{
    public class FieldMapConfigJsonConverter : JsonCreationConverter<IFieldMapConfig>
    {
        private bool FieldExists(string fieldName, JObject j)
        {
            return j[fieldName] != null;
        }

        protected override IFieldMapConfig Create(Type objectType, JObject j)
        {
            if (FieldExists("ObjectType", j))
            {
                // Initialize.
                string typeName;

                // Read the type stored.
                string value = j.GetValue("ObjectType").ToString();

                // Validate if it is a full or relative type.
                if(value.StartsWith("VstsSyncMigrator.Engine.Configuration.FieldMap."))
                    typeName = value;
                else
                    typeName = $"VstsSyncMigrator.Engine.Configuration.FieldMap.{value}";

                // Get the right type.
                Type type = Type.GetType(typeName, true);

                // Create an instance of that type.
                return (IFieldMapConfig)Activator.CreateInstance(type);
            }
            else
            {
                throw new NotImplementedException($"field 'ObjectType' does not exist in JObject {j.ToString()}");
            }
        }
    }
}


