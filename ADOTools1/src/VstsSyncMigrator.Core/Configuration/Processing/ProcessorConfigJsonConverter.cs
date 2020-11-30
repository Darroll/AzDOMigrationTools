using System;
using Newtonsoft.Json.Linq;
using VstsSyncMigrator.Engine.Configuration.Processing;

namespace VstsSyncMigrator.Engine.Configuration.FieldMap
{
    public class ProcessorConfigJsonConverter : JsonCreationConverter<ITfsProcessingConfig>
    {
        protected override ITfsProcessingConfig Create(Type objectType, JObject j)
        {
            if (FieldExists("ObjectType", j))
            {
                // Initialize.
                string typeName;

                // Read the type stored.
                string value = j.GetValue("ObjectType").ToString();

                // Validate if it is a full or relative type.
                if (value.StartsWith("VstsSyncMigrator.Engine.Configuration.Processing."))
                    typeName = value;
                else
                    typeName = $"VstsSyncMigrator.Engine.Configuration.Processing.{value}";

                // Get the right type.
                Type type = Type.GetType(typeName, true);

                // Create an instance of that type.
                return (ITfsProcessingConfig)Activator.CreateInstance(type);
            }
            else
            {
                throw new NotImplementedException($"field 'ObjectType' does not exist in JObject {j.ToString()}");
            }
        }
        private bool FieldExists(string fieldName, JObject o)
        {
            return o[fieldName] != null;
        }
    }
}


