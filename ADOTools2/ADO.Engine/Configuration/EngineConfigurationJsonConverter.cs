using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ADO.Engine.Configuration
{
    public class EngineConfigurationJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            // Initialize.
            bool value = false;
            
            // Can convert these types only.
            value = typeof(string).IsAssignableFrom(objectType);
            value = value || typeof(ProjectExportBehavior).IsAssignableFrom(objectType);
            value = value || typeof(ProjectImportBehavior).IsAssignableFrom(objectType);
            value = value || typeof(RestApiServiceConfig).IsAssignableFrom(objectType);
            value = value || typeof(ProjectExport.EngineConfiguration).IsAssignableFrom(objectType);
            value = value || typeof(ProjectImport.EngineConfiguration).IsAssignableFrom(objectType);

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
                if (objectType.FullName == "ADO.Engine.Configuration.ProjectExport.EngineConfiguration")
                {
                    // Load json object from reader.
                    item = JObject.Load(reader);

                    generatedObject = new ProjectExport.EngineConfiguration();
                    callPopulate = true;
                }
                else if (objectType.FullName == "ADO.Engine.Configuration.ProjectImport.EngineConfiguration")
                {
                    // Load json object from reader.
                    item = JObject.Load(reader);

                    generatedObject = new ProjectImport.EngineConfiguration();
                    callPopulate = true;
                }
                else if (objectType.Name == "ProjectExportBehavior")
                {
                    if (reader.TokenType == JsonToken.Null)
                        generatedObject = ProjectExportBehavior.GetDefault(false);
                    else
                    {
                        // Load json object from reader.
                        item = JObject.Load(reader);

                        generatedObject = new ProjectExportBehavior();
                        callPopulate = true;
                    }
                }
                else if (objectType.Name == "ProjectImportBehavior")
                {
                    if (reader.TokenType == JsonToken.Null)
                        generatedObject = ProjectImportBehavior.GetDefault(false);
                    else
                    {
                        // Load json object from reader.
                        item = JObject.Load(reader);

                        generatedObject = new ProjectImportBehavior();
                        callPopulate = true;
                    }
                }
                else if (objectType.Name == "RestApiServiceConfig")
                {
                    if (reader.TokenType == JsonToken.Null)
                        generatedObject = RestApiServiceConfig.GetDefault();
                    else
                    {
                        // Load json object from reader.
                        item = JObject.Load(reader);

                        generatedObject = new RestApiServiceConfig();
                        callPopulate = true;
                    }
                }

                // Populate the values into generated object.
                if (callPopulate)
                    serializer.Populate(item.CreateReader(), generatedObject);

                // Track the state of this object before leaving.
                if (objectType.FullName == "ADO.Engine.Configuration.ProjectExport.EngineConfiguration")
                {
                    ProjectExport.EngineConfiguration config = (ProjectExport.EngineConfiguration)generatedObject;

                    // If not defined, set default.
                    if (config.Behaviors == null)
                        config.Behaviors = ProjectExportBehavior.GetDefault(false);
                    else
                        config.Behaviors.SetDefaultIfUndefined();

                    // If not defined, set default.
                    if (config.RestApiService == null)
                        config.RestApiService = RestApiServiceConfig.GetDefault();
                    else
                        config.RestApiService.SetDefaultIfUndefined();

                    // Force validation.
                    config.Validate();

                    // Save back.
                    generatedObject = config;
                }
                else if (objectType.FullName == "ADO.Engine.Configuration.ProjectImport.EngineConfiguration")
                {
                    ProjectImport.EngineConfiguration config = (ProjectImport.EngineConfiguration)generatedObject;

                    // If not defined, set default.
                    if (config.Behaviors == null)
                        config.Behaviors = ProjectImportBehavior.GetDefault(false);
                    else
                        config.Behaviors.SetDefaultIfUndefined();

                    // If not defined, set default.
                    if (config.RestApiService == null)
                        config.RestApiService = RestApiServiceConfig.GetDefault();
                    else
                        config.RestApiService.SetDefaultIfUndefined();

                    // Force validation.
                    config.Validate();

                    // Save back.
                    generatedObject = config;
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
