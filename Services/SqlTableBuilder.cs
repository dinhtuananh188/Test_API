using DocumentFormat.OpenXml.Spreadsheet;
using System.Data;
using System.Text.RegularExpressions;

namespace Test_API.Services
{
    public class SqlTableBuilder
    {
        public string BuildCreateTableSql(string tableName, DataTable dt)
        {
            var columnDefs = new List<string>();
            // Chỉ thêm id SERIAL PRIMARY KEY nếu DataTable không có cột id
            bool hasIdColumn = dt.Columns.Cast<DataColumn>().Any(c => c.ColumnName.Trim().ToLower() == "id");
            if (!hasIdColumn)
            {
                columnDefs.Add("id SERIAL PRIMARY KEY");
            }
            foreach (DataColumn col in dt.Columns)
            {
                string rawName = col.ColumnName.ToString();
                string colName = Regex.Replace(rawName.Trim(), @"\s+", "_");

                string lowerColName = colName.ToLower();
                string colType;
                if (lowerColName.Contains("date") || lowerColName.Contains("study_from") || lowerColName.Contains("study_to"))
                {
                    colType = "DATE";
                }
                else
                {
                    colType = "TEXT";
                }
                columnDefs.Add($"\"{colName}\" {colType}");
            }
            return $"CREATE TABLE IF NOT EXISTS \"{tableName}\" (" + string.Join(", ", columnDefs) + ")";
        }
    }
}
