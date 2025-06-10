using System.Data;

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
                string colName = col.ColumnName.Replace(" ", "_");
                string colType = "TEXT"; // Cải tiến sau nếu cần
                columnDefs.Add($"\"{colName}\" {colType}");
            }
            return $"CREATE TABLE IF NOT EXISTS \"{tableName}\" (" + string.Join(", ", columnDefs) + ")";
        }
    }
}
