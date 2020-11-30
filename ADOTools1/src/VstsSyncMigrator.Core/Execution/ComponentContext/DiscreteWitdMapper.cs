namespace VstsSyncMigrator.Engine
{
    public class DiscreteWitdMapper : IWitdMapper
    {
        #region - Private Members

        private readonly string _discreteName;

        #endregion

        #region - Public Members

        public string DiscreteMapValue
        {
            get { return _discreteName; }
        }

        public DiscreteWitdMapper(string witdName)
        {
            _discreteName = witdName;
        }

        #endregion
    }
}