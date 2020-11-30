using LINQtoCSV;

namespace ADO.Engine.BusinessEntities
{
    public class BusinessHierarchyCsv
    {
        [CsvColumn(FieldIndex = 1, CanBeNull = false, Name = "Collection")]
        public string OrganizationOrCollection { get; set; }

        [CsvColumn(FieldIndex = 2, CanBeNull = false, Name = "Project")]
        public string Project { get; set; }

        [CsvColumn(FieldIndex = 3, CanBeNull = true, Name = "Portfolio")]
        public string Portfolio { get; set; }

        [CsvColumn(FieldIndex = 4, CanBeNull = true, Name = "ProgramOrProduct")]
        public string ProgramOrProduct { get; set; }

        [CsvColumn(FieldIndex = 5, CanBeNull = true, Name = "Prefix")]
        public string Prefix { get; set; }

        [CsvColumn(FieldIndex = 6, CanBeNull = false, Name = "Process")]
        public string Process { get; set; }

        [CsvColumn(FieldIndex = 7, CanBeNull = true, Name = "TeamInclusionList")]
        public string TeamInclusionList { get; set; }

        [CsvColumn(FieldIndex = 8, CanBeNull = true, Name = "AreaPrefixPath")]
        public string AreaPrefixPath { get; set; }

        [CsvColumn(FieldIndex = 9, CanBeNull = true, Name = "AreaBasePathList")]
        public string AreaBasePathList { get; set; }

        [CsvColumn(FieldIndex = 10, CanBeNull = true, Name = "AreaPathMapDictionary")]
        public string AreaPathMapDictionary { get; set; }

        [CsvColumn(FieldIndex = 11, CanBeNull = true, Name = "IterationPrefixPath")]
        public string IterationPrefixPath { get; set; }

        [CsvColumn(FieldIndex = 12, CanBeNull = true, Name = "IterationBasePathList")]
        public string IterationBasePathList { get; set; }

        [CsvColumn(FieldIndex = 13, CanBeNull = true, Name = "IterationPathMapDictionary")]
        public string IterationPathMapDictionary { get; set; }

        [CsvColumn(FieldIndex = 14, CanBeNull = false, Name = "IsClone")]
        public bool IsClone { get; set; }

        [CsvColumn(FieldIndex = 15, CanBeNull = false, Name = "IsOnPremiseProject")]
        public bool IsOnPremiseProject { get; set; }

        [CsvColumn(FieldIndex = 16, CanBeNull = true, Name = "AzureDevOpsServerFQDN")]
        public string AzureDevOpsServerFQDN { get; set; }
    }
}
