using System.IO;

namespace VstsSyncMigrator.Engine
{
    public abstract class AttachementMigrationContextBase : MigrationContextBase
    {
        #region - Private Members

        private string _exportPath;

        private void EnsureExportPath()
        {
            if (_exportPath == null)
            {
                string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                // Use the assembly path as the root and create an export folder under it.
                _exportPath = Path.Combine(Path.GetDirectoryName(assemblyPath), "export");
                Directory.CreateDirectory(_exportPath);
            }
        }

        #endregion

        #region - Public Members

        public string ExportPath
        {
            get { return _exportPath; }
        }

        public AttachementMigrationContextBase(MigrationEngine me) : base(me)
        {
            EnsureExportPath();
        }

        #endregion
    }
}