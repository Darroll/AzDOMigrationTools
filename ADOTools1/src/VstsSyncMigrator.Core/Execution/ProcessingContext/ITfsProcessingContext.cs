namespace VstsSyncMigrator.Engine
{
    public interface ITfsProcessingContext
    {
        string Name { get; }

        ProcessingStatus Status { get; }

        void Execute();
    }
}
