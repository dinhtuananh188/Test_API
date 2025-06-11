using Dapper;
using Npgsql;
using System.Text.Json;
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

        public async Task<int> InsertDataAsync(string tableName, List<Dictionary<string, object>> data, List<string> debugInfo)
        {
            int insertedRows = 0;
            var builder = new SqlTableBuilder();
            string createSql = builder.BuildCreateTableSql(tableName, data);

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                debugInfo.Add(createSql);
                await conn.ExecuteAsync(createSql);

                // Chuyển dữ liệu thành JSON và log
                string jsonData = JsonSerializer.Serialize(data);

                // Gọi stored procedure với OUT parameter
                var parameters = new DynamicParameters();
                parameters.Add("p_table_name", tableName, dbType: DbType.String);
                parameters.Add("p_json_data", jsonData, dbType: DbType.String);
                parameters.Add("p_row_count", 0, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);

                // Gọi stored procedure trực tiếp với CALL
                await conn.ExecuteAsync(
                    "CALL insert_from_json(@p_table_name, @p_json_data::json, @p_row_count)",  // ép kiểu JSON
                    parameters,
                    commandType: CommandType.Text
                );


                // Lấy giá trị từ tham số OUT
                insertedRows = parameters.Get<int>("p_row_count");
                debugInfo.Add($"Inserted {insertedRows} rows via procedure");
            }

            return insertedRows;
        }
    }
}