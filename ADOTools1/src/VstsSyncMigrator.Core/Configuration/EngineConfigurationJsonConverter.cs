using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using VstsSyncMigrator.Engine.Configuration;

namespace VstsSyncMigrator.Core.Configuration
{
    public class EngineConfigurationJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            // Initialize.
            bool value = false;

            // Can convert these types only.
            value = typeof(string).IsAssignableFrom(objectType);
            value = value || typeof(Engine.Configuration.EngineConfiguration).IsAssignableFrom(objectType);

            // Is it convertable.
            return value;
        }

        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Initialize.
            bool callPopulate = false;
            object generatedObject = null;
            JObject item = null;

            if (objectType.Name == "String")
            {
                if (reader.TokenType == JsonToken.Null)
                    generatedObject = null;
                else
                    // No deserialization is needing, just conversion.
                    generatedObject = (string)reader.Value;
            }
            else
            {
                if (objectType.FullName == "VstsSyncMigrator.Engine.Configuration.EngineConfiguration")
                {
                    // Load json object from reader.
                    item = JObject.Load(reader);

                    generatedObject = new EngineConfiguration();
                    callPopulate = true;
                }
                else
                {
                    throw new NotImplementedException("too doo");
                }

                // Populate the values into generated object.
                if (callPopulate)
                    serializer.Populate(item.CreateReader(), generatedObject);

                // Track the state of this object before leaving.
                if (objectType.FullName == "VstsSyncMigrator.Engine.Configuration.EngineConfiguration")
                {
                    EngineConfiguration config = (EngineConfiguration)generatedObject;

                    // If not defined, set default.
                    
                    // Force validation.
                    config.Validate();

                    // Save back.
                    generatedObject = config;
                }
                else 
                {
                    throw new NotImplementedException("too doo");
                }
            }

            // Return object.
            return generatedObject;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("should not need this");
        }
    }
}
