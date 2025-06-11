using ClosedXML.Excel;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Data;

namespace Test_API.Services
{
    public class ExcelParser
    {
        public List<Dictionary<string, object>> Parse(Stream fileStream)
        {
            var result = new List<Dictionary<string, object>>();
            var dateColumnNames = new List<string>();

            using (var workbook = new XLWorkbook(fileStream))
            {
                var worksheet = workbook.Worksheets.First();
                bool firstRow = true;
                string[] columnNames = null;

                foreach (var row in worksheet.RowsUsed())
                {
                    if (firstRow)
                    {
                        columnNames = row.Cells()
                            .Select(cell => Regex.Replace(cell.Value.ToString().Trim(), @"\s+", "_"))
                            .ToArray();

                        // Tìm các cột liên quan đến thời gian
                        for (int i = 0; i < columnNames.Length; i++)
                        {
                            string lowerName = columnNames[i].ToLower();
                            if (lowerName.Contains("date") || lowerName.Contains("study_from") || lowerName.Contains("study_to"))
                            {
                                dateColumnNames.Add(columnNames[i]);
                            }
                        }
                        firstRow = false;
                    }
                    else
                    {
                        var dict = new Dictionary<string, object>();
                        var cells = row.Cells(1, columnNames.Length).ToList();

                        for (int i = 0; i < columnNames.Length; i++)
                        {
                            string cellText = i < cells.Count ? cells[i].GetFormattedString().Trim() : string.Empty;

                            if (dateColumnNames.Contains(columnNames[i]))
                            {
                                DateTime parsedDate;
                                string[] formats = { "d/M/yyyy", "dd/MM/yyyy", "dd/M/yyyy", "d/MM/yyyy" };

                                if (DateTime.TryParseExact(cellText, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate)
                                    || DateTime.TryParse(cellText, out parsedDate))
                                {
                                    dict[columnNames[i]] = parsedDate.ToString("M/d/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                                }
                                else
                                {
                                    dict[columnNames[i]] = null;
                                }
                            }
                            else
                            {
                                dict[columnNames[i]] = string.IsNullOrEmpty(cellText) ? null : cellText;
                            }
                        }
                        result.Add(dict);
                    }
                }
            }

            return result;
        }
    }
}