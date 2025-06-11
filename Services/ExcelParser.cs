using ClosedXML.Excel;
using System.Data;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Test_API.Services
{
    public class ExcelParser
    {
        public DataTable Parse(Stream fileStream)
        {
            var dt = new DataTable();
            var dateColumnIndexes = new List<int>();

            using (var workbook = new XLWorkbook(fileStream))
            {
                var worksheet = workbook.Worksheets.First();
                bool firstRow = true;

                foreach (var row in worksheet.RowsUsed())
                {
                    if (firstRow)
                    {
                        int colIndex = 0;
                        foreach (var cell in row.Cells())
                        {
                            string rawName = cell.Value.ToString();
                            string colName = Regex.Replace(rawName.Trim(), @"\s+", "_");
                            dt.Columns.Add(colName);

                            // Tìm các cột có tên liên quan đến thời gian
                            string lowerName = colName.ToLower();
                            if (lowerName.Contains("date") || lowerName.Contains("study_from") || lowerName.Contains("study_to"))
                            {
                                dateColumnIndexes.Add(colIndex);
                            }
                            colIndex++;
                        }
                        firstRow = false;
                    }
                    else
                    {
                        var values = new object[dt.Columns.Count];
                        var cells = row.Cells(1, dt.Columns.Count).ToList();

                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            string cellText = i < cells.Count ? cells[i].GetFormattedString().Trim() : string.Empty;

                            if (dateColumnIndexes.Contains(i))
                            {
                                DateTime parsedDate;

                                // Dùng TryParseExact với nhiều định dạng ngày phổ biến
                                string[] formats = {
                                    "d/M/yyyy", "dd/MM/yyyy", "dd/M/yyyy", "d/MM/yyyy"
                                };

                                if (DateTime.TryParseExact(cellText, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate)
                                    || DateTime.TryParse(cellText, out parsedDate)) // fallback
                                {
                                    values[i] = parsedDate.ToString("M/d/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                                }
                                else
                                {
                                    values[i] = DBNull.Value;
                                }
                            }
                            else
                            {
                                values[i] = string.IsNullOrEmpty(cellText) ? DBNull.Value : cellText;
                            }
                        }

                        dt.Rows.Add(values);
                    }
                }
            }

            return dt;
        }
    }
}
