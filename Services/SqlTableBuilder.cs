using System.Text.RegularExpressions;

namespace Test_API.Services
{
    public class SqlTableBuilder
    {
        public string BuildCreateTableSql(string tableName, List<Dictionary<string, object>> data)
        {
            var columnDefs = new List<string>();
            bool hasIdColumn = false;

            if (data.Any())
            {
                var firstRow = data.First();
                hasIdColumn = firstRow.Keys.Any(k => k.Trim().ToLower() == "id");
                if (!hasIdColumn)
                {
                    columnDefs.Add("id SERIAL PRIMARY KEY");
                }

                foreach (var key in firstRow.Keys)
                {
                    string colName = Regex.Replace(key.Trim(), @"\s+", "_");
                    string lowerColName = colName.ToLower();
                    string colType = lowerColName.Contains("date") || lowerColName.Contains("study_from") || lowerColName.Contains("study_to")
                        ? "DATE"
                        : "TEXT";
                    columnDefs.Add($"\"{colName}\" {colType}");
                }
            }

            return $"CREATE TABLE IF NOT EXISTS \"{tableName}\" (" + string.Join(", ", columnDefs) + ")";
        }
    }
}