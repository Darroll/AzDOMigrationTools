using Newtonsoft.Json;

namespace ADO.Engine.Configuration
{
    public sealed class ProjectImportSourceCodeCredentials
    {
        #region - Static Declarations

        #region - Public Members

        public static ProjectImportSourceCodeCredentials GetDefault()
        {
            ProjectImportSourceCodeCredentials config = new ProjectImportSourceCodeCredentials
            {
                UserID = "<Enter your user identifier to connect to ADO>",
                Password = "<Enter your password to connect to ADO>",
                GitUsername = "<Enter your user identifier to connect to GitHub>",
                GitPassword = "<Enter your password to connect to GitHub>"
            };

            // Return default configuration.
            return config;
        }

        #endregion

        #endregion

        #region - Public Members

        [JsonProperty(PropertyName = "userID")]
        public string UserID { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }

        [JsonProperty(PropertyName = "gitUsername")]
        public string GitUsername { get; set; }

        [JsonProperty(PropertyName = "gitPassword")]
        public string GitPassword { get; set; }

        #endregion
    }
}
