using Newtonsoft.Json.Linq;

namespace ADO.RestAPI
{
    public static class Constants
    {
        public const string Agile = "adcc42ab-9882-485e-a3ed-7678f01f66bc";
        public const string SCRUM = "6b724908-ef14-45cf-84f8-768b5384da45";
        public const string CMMI = "27450541-8e31-4150-9947-dc59f998fc01";
        public const string DestinationProcessTemplate = "e4d7cdae-534c-4566-b295-16db00155d03";
        public const string Queue = "Hosted";

        public static readonly string BuildNamespaceId = "33344d9c-fc72-4d6f-aba5-fa317101a7e9";
        public static readonly string CSSNamespaceId = "83e28ad4-2d72-4ceb-97b0-c7726d5502c3";
        public static readonly string DistributedTaskEnvironmentId = "83d4c2e6-e57d-4d6e-892b-b87222b7ad20";
        public static readonly string DistributedTaskLibraryId = "b7e84409-6553-448a-bbb2-af228e07cbeb";
        public static readonly string MetaTaskId = "f6a4de49-dbe2-4704-86dc-f8ec1a294436";
        public static readonly string ProjectNamespaceId = "52d39943-cb85-4d7f-8fa8-c6baac873819";
        public static readonly string QueryFolderNamespaceId = "71356614-aad7-4757-8f2c-0fb3bff6f680";
        public static readonly string ReleaseNamespaceId = "c788c23e-1b46-4162-8f5e-d7585343b5de";
        public static readonly string RepoSecNamespaceId = "2e9eb7ed-3c0a-47d4-87c1-0ffdd275fd87";

        //errors (treated as warnings)
        public static readonly string ErrorVS402371 = "VS402371";
        public static readonly string ErrorTF401243 = "TF401243";

        // TODO: Move this to a template file
        public static readonly string RedirectedTaskBuild = "{\"environment\":{},\"displayName\":\"$RedirectedName$\",\"alwaysRun\":false,\"continueOnError\":false,\"condition\":\"succeeded()\",\"enabled\":true,\"timeoutInMinutes\":0,\"inputs\":{\"targetType\":\"inline\",\"script\":$RedirectedContent$,\"errorActionPreference\":\"stop\",\"failOnStderr\":\"false\",\"ignoreLASTEXITCODE\":\"false\",\"pwsh\":\"false\",\"workingDirectory\":\"$(System.DefaultWorkingDirectory)\"},\"task\":{\"id\":\"$Task:PowerShell-Id$\",\"versionSpec\":\"2.*\",\"definitionType\":\"task\"}}";
        public static readonly string RedirectedTaskRelease = "{\"environment\":{},\"taskId\":\"$Task:PowerShell-Id$\",\"version\":\"2.*\",\"name\":\"$RedirectedName$\",\"refName\":\"\",\"enabled\":true,\"alwaysRun\":false,\"continueOnError\":false,\"timeoutInMinutes\":0,\"definitionType\":\"task\",\"overrideInputs\":{},\"condition\":\"succeeded()\",\"inputs\":{\"targetType\":\"inline\",\"script\":$RedirectedContent$,\"errorActionPreference\":\"stop\",\"failOnStderr\":\"false\",\"ignoreLASTEXITCODE\":\"false\",\"pwsh\":\"false\",\"workingDirectory\":\"$(System.DefaultWorkingDirectory)\"}}";

        public static readonly string AgileTemplateType = "Agile";
        public static readonly string ScrumTemplateType = "Scrum";
        public static readonly string CmmiTemplateType = "CMMI";
        
        public static readonly string Bugs = "Bugs";
        public static readonly string Bug = "Bug";

        public static readonly string DefaultPathSeparator = "\\";
        public static readonly string DefaultPathSeparatorForward = "/";
        public static readonly JValue DefaultAgentPoolNameForOnPrem = new JValue("Default");
        public static readonly JValue DefaultAgentPoolNameForOnCloud = new JValue("Hosted VS2017");
    }
}
