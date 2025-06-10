using ClosedXML.Excel;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Data;
using System.Text.Json;
using Test_API.DTOs;
using Test_API.Services;

[ApiController]
[Route("api/[controller]")]
public class ExcelUploadController : ControllerBase
{
    private readonly IConfiguration _config;

    public ExcelUploadController(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadExcel([FromForm] ExcelUploadRequest request)
    {
        var file = request.File;
        var tableName = request.TableName;
        if (file == null || file.Length == 0)
            return BadRequest(new { success = false, message = "No file uploaded" });

        string generatedTableName = string.IsNullOrWhiteSpace(tableName)
            ? $"excel_{DateTime.UtcNow:yyyyMMddHHmmssfff}"
            : tableName;

        var debugInfo = new List<string>();
        try
        {
            var parser = new ExcelParser();
            var dt = parser.Parse(file.OpenReadStream());

            if (dt.Columns.Count == 0)
                return BadRequest(new { success = false, message = "Excel file has no columns" });

            var inserter = new DatabaseInserter(_config.GetConnectionString("DefaultConnection"));
            int insertedRows = await inserter.InsertDataAsync(generatedTableName, dt, debugInfo);

            return Ok(new
            {
                success = true,
                insertedRows,
                tableName = generatedTableName,
                json = ConvertDataTableToList(dt),
                debug = debugInfo
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message,
                debug = debugInfo
            });
        }
    }

    private List<Dictionary<string, object>> ConvertDataTableToList(DataTable dt)
    {
        var list = new List<Dictionary<string, object>>();
        foreach (DataRow row in dt.Rows)
        {
            var dict = new Dictionary<string, object>();
            foreach (DataColumn col in dt.Columns)
            {
                dict[col.ColumnName] = row[col] is DBNull ? null : row[col];
            }
            list.Add(dict);
        }
        return list;
    }
}

