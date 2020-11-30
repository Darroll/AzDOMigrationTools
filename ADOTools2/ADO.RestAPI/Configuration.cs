using System;
using ADO.Extensions;

namespace ADO.RestAPI
{
    public sealed class Configuration : IConfiguration
    {
        #region - Properties implemented via the IConfiguration interface.

        public string BaseUri { get; set; }

        public string Collection { get; set; }

        public string PersonalAccessToken { get; set; }

        public string Project { get; set; }

        public string Version { get; set; }

        #endregion

        #region - Constructors 

        // Constructor that takes care of generating the uri string.
        public Configuration(ServiceHost sh, string collection, string project, string accessToken, string apiVersion)
        {
            // Assign collection and project properties directly.
            this.Collection = collection;
            this.Project = project;

            // Obtain the host address via enum description.
            Uri combinedUri = new Uri(new Uri(sh.GetDescription()), collection);

            // Assign BaseUri property directly.
            this.BaseUri = combinedUri.ToString();

            // Set the version of api to use.
            this.Version = apiVersion;

            // Assign the access token.
            this.PersonalAccessToken = accessToken;
        }

        public Configuration(ServiceHost sh, IConfiguration sourceConfig)
        {
            // Assign collection and project properties directly.
            this.Collection = sourceConfig.Collection;
            this.Project = sourceConfig.Project;

            // Obtain the host address via enum description.
            Uri combinedUri = new Uri(new Uri(sh.GetDescription()), sourceConfig.Collection);

            // Assign BaseUri property directly.
            this.BaseUri = combinedUri.ToString();

            // Set the version of api to use.
            this.Version = sourceConfig.Version;

            // Assign the access token.
            this.PersonalAccessToken = sourceConfig.PersonalAccessToken;
        }

        #endregion
    }
}