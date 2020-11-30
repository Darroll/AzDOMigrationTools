using System.ComponentModel;

namespace ADO.RestAPI
{
    ///// <summary>
    ///// Possible Azure DevOps REST api service hosts.
    ///// Each corresponds to different service areas.
    ///// </summary>
    //public enum ServiceHost
    //{
    //    [Description("https://dev.azure.com/")]
    //    DefaultHost = 0,
    //    [Description("https://app.vssps.visualstudio.com/")]
    //    ApplicationHost = 1,
    //    [Description("https://vssps.dev.azure.com/")]
    //    GraphHost = 2,
    //    [Description("https://vsrm.dev.azure.com/")]
    //    ReleaseHost = 3,
    //    [Description("https://vsaex.dev.azure.com/")]
    //    EntitlementHost = 4,
    //    [Description("https://extmgmt.dev.azure.com/")]
    //    ExtensionManagementHost = 5,
    //    [Description("https://feeds.dev.azure.com/")]
    //    FeedHost = 6,
    //    [Description("https://artifacts.dev.azure.com/")]
    //    ArtifactsHost = 7,
    //    [Description("https://vsclt.dev.azure.com/")]
    //    CloudLoadTestHost = 8,
    //    [Description("https://almsearch.dev.azure.com/")]
    //    ALMSearchHost = 9
    //}

    /// <summary>
    /// Possible Azure DevOps REST api service hosts.
    /// Each corresponds to different service areas.
    /// </summary>
    public enum ServiceHost
    {
        [Description("https://devops.peoc3t.com/")]
        DefaultHost = 0,
        //[Description("https://app.vssps.visualstudio.com/")]
        //ApplicationHost = 1,
        [Description("https://devops.peoc3t.com/")]
        GraphHost = 2,
        [Description("https://devops.peoc3t.com/")]
        ReleaseHost = 3,
        //[Description("https://vsaex.dev.azure.com/")]
        //EntitlementHost = 4,
        [Description("https://devops.peoc3t.com/")]
        ExtensionManagementHost = 5,
        //[Description("https://feeds.dev.azure.com/")]
        //FeedHost = 6,
        //[Description("https://artifacts.dev.azure.com/")]
        //ArtifactsHost = 7,
        //[Description("https://vsclt.dev.azure.com/")]
        //CloudLoadTestHost = 8,
        //[Description("https://almsearch.dev.azure.com/")]
        //ALMSearchHost = 9
    }
}
