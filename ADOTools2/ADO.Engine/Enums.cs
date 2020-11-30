using System.ComponentModel;

namespace ADO.Engine
{
    /// <summary>
    /// Possible Azure DevOps identity types to define identity descriptor.
    /// </summary>
    public enum IdentityType
    {
        [Description("Microsoft.TeamFoundation.Identity")]
        MicrosoftTeamFoundationIdentity = 0,
        [Description("Microsoft.TeamFoundation.ServiceIdentity")]
        MicrosoftTeamFoundationServiceIdentity = 1,
    }

    public enum OperationLocation
    {
        Source,
        Destination
    }

    /// <summary>
    /// Possible Azure DevOps process templates.
    /// </summary>
    public enum ProcessTemplateType
    {
        Agile,
        Scrum,
        CMMI
    }

    public enum ReplacementTokenValueType
    {
        Name,
        Id,
        DisplayName,
        UniqueName
    }
}
