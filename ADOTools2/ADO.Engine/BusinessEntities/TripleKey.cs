using ADO.Engine.Utilities;
using System.ComponentModel;

namespace ADO.Engine.BusinessEntities
{
    [TypeConverter(typeof(TripleKeyTypeConverter))]
    public class TripleKey
    {

        public string Organization { get; set; }
        public string Project { get; set; }
        public string Path { get; set; }

        public TripleKey(string organization, string project, string path)
        {
            Organization = organization;
            Project = project;
            Path = path;
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.Organization.Equals(((TripleKey)obj).Organization) &&
                this.Project.Equals(((TripleKey)obj).Project) &&
                this.Path.Equals(((TripleKey)obj).Path);
        }

        public override string ToString()
        {
            return $"({this.Organization}, {this.Project}, {this.Path})";
        }
    }
}