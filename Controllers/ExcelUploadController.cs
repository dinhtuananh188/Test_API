using Microsoft.AspNetCore.Mvc;
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
            var jsonData = parser.Parse(file.OpenReadStream());

            if (!jsonData.Any())
                return BadRequest(new { success = false, message = "Excel file has no data" });

            var inserter = new DatabaseInserter(_config.GetConnectionString("DefaultConnection"));
            int insertedRows = await inserter.InsertDataAsync(generatedTableName, jsonData, debugInfo);

            return Ok(new
            {
                success = true,
                insertedRows,
                tableName = generatedTableName,
                json = jsonData,
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
}