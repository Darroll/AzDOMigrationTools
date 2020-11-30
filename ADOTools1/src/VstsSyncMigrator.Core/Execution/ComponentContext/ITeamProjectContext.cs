using Microsoft.TeamFoundation.Client;

namespace VstsSyncMigrator.Engine
{
    public interface ITeamProjectContext
    {
        TfsTeamProjectCollection Collection { get; }

        string Name { get; }

        void Connect();
    }
}