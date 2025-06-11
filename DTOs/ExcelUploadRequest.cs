using Microsoft.AspNetCore.Mvc;

namespace Test_API.DTOs
{
    public class ExcelUploadRequest
    {
        [FromForm]
        public IFormFile? File { get; set; }
        [FromForm]
        public string? TableName { get; set; }
    }
}
