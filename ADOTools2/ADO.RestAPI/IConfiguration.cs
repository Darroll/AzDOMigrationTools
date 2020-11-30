namespace ADO.RestAPI
{
    public interface IConfiguration
    {
        string BaseUri { get; set; }

        string Collection { get; set; }

        string PersonalAccessToken { get; set; }

        string Project { get; set; }

        string Version { get; set; }
    }
}