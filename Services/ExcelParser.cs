using ClosedXML.Excel;
using System.Data;

namespace Test_API.Services
{
    public class ExcelParser
    {
        public DataTable Parse(Stream fileStream)
        {
            var dt = new DataTable();
            using (var workbook = new XLWorkbook(fileStream))
            {
                var worksheet = workbook.Worksheets.First();
                bool firstRow = true;
                foreach (var row in worksheet.RowsUsed())
                {
                    if (firstRow)
                    {
                        foreach (var cell in row.Cells())
                            dt.Columns.Add(cell.Value.ToString());
                        firstRow = false;
                    }
                    else
                    {
                        // Convert each cell value to string or DBNull.Value
                        var values = new object[dt.Columns.Count];
                        var cells = row.Cells(1, dt.Columns.Count).ToList();
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            string cellText = i < cells.Count ? cells[i].Value.ToString() : string.Empty;
                            values[i] = string.IsNullOrEmpty(cellText) ? DBNull.Value : cellText;
                        }
                        dt.Rows.Add(values);
                    }
                }
            }
            return dt;
        }
    }
}
