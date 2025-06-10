using Dapper;
using Npgsql;
using System.Data;

namespace Test_API.Services
{
    public class DatabaseInserter
    {
        private readonly string _connectionString;

        public DatabaseInserter(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> InsertDataAsync(string tableName, DataTable dt, List<string> debugInfo)
        {
            int insertedRows = 0;
            var builder = new SqlTableBuilder();
            string createSql = builder.BuildCreateTableSql(tableName, dt);

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                debugInfo.Add(createSql);
                await conn.ExecuteAsync(createSql);

                foreach (DataRow row in dt.Rows)
                {
                    var nonIdColumns = dt.Columns.Cast<DataColumn>().Where(c => c.ColumnName.ToLower() != "id").ToList();
                    var colNames = nonIdColumns.Select(c => $"\"{c.ColumnName.Replace(" ", "_")}\"");
                    var paramNames = nonIdColumns.Select((c, i) => $"@p{i}");
                    string insertSql = $"INSERT INTO \"{tableName}\" (" + string.Join(", ", colNames) + ") VALUES (" + string.Join(", ", paramNames) + ")";
                    var param = new DynamicParameters();
                    int paramIndex = 0;
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        if (dt.Columns[i].ColumnName.ToLower() == "id") continue;
                        var value = row[i] is DBNull ? null : row[i];
                        param.Add($"p{paramIndex}", value);
                        paramIndex++;
                    }

                    try
                    {
                        await conn.ExecuteAsync(insertSql, param);
                        insertedRows++;
                    }
                    catch (Exception ex)
                    {
                        debugInfo.Add($"Insert error: {ex.Message}");
                    }
                }
            }

            return insertedRows;
        }
    }
}
