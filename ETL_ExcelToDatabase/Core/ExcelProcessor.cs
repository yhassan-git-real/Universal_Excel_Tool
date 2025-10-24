using ClosedXML.Excel;
using System.Data;
using Microsoft.Data.SqlClient;
using ETL_ExcelToDatabase.Models;
using ETL_ExcelToDatabase.Services;

namespace ETL_ExcelToDatabase.Core
{
    public static class ExcelProcessor
    {
        public static IEnumerable<BatchResult> LoadExcelInBatches(
    string excelFilePath,
    SqlConnection connection,
    string errorTableName,
    int batchSize,
    string defaultSheetName)
        {
            using var workbook = new XLWorkbook(excelFilePath);
            var worksheet = !string.IsNullOrEmpty(defaultSheetName) && workbook.Worksheets.Contains(defaultSheetName)
                ? workbook.Worksheet(defaultSheetName)
                : workbook.Worksheet(1);

            ConsoleLogger.LogInfo("file", $"Processing sheet: {worksheet.Name}");

            var usedRange = worksheet.RangeUsed();
            if (usedRange == null)
            {
                throw new ArgumentException("Excel file is empty.");
            }

            var headers = usedRange.FirstRow()
                .CellsUsed()
                .Select(cell => cell.Value.ToString())
                .ToArray();

            var currentBatch = CreateDataTable(headers);
            bool isFirstBatch = true;
            int totalRows = usedRange.RowCount() - 1;
            int processedRows = 0;

            // Process each row after the header
            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                if (row.IsEmpty()) continue;

                bool rowProcessedSuccessfully = true;
                try
                {
                    ProcessExcelRow(row, currentBatch, headers.Length);
                    processedRows++;

                    if (processedRows % 50000 == 0)
                    {
                        ConsoleLogger.LogProgress("Processing Excel rows", processedRows, totalRows);
                    }
                }
                catch (Exception ex)
                {
                    rowProcessedSuccessfully = false;
                    // Log error and continue processing
                    _ = LoggingService.LogError(
                        connection,
                        errorTableName,
                        Path.GetFileName(excelFilePath),
                        $"Row {row.RowNumber()}",
                        "Data Processing Error",
                        ex.Message);

                    ConsoleLogger.LogError($"Error processing row {row.RowNumber()}: {ex.Message}");
                }

                // Only check batch size if row was processed successfully
                if (rowProcessedSuccessfully && currentBatch.Rows.Count >= batchSize)
                {
                    yield return new BatchResult
                    {
                        Data = currentBatch,
                        IsFirstBatch = isFirstBatch,
                        IsLastBatch = false
                    };
                    currentBatch = CreateDataTable(headers);
                    isFirstBatch = false;
                }
            }

            // Return the final batch if it contains any rows
            if (currentBatch.Rows.Count > 0)
            {
                yield return new BatchResult
                {
                    Data = currentBatch,
                    IsFirstBatch = isFirstBatch,
                    IsLastBatch = true
                };
            }

            ConsoleLogger.LogSuccess($"Completed processing {processedRows:N0} rows");
        }

        private static DataTable CreateDataTable(string[] headers)
        {
            DataTable dt = new();
            foreach (string header in headers)
            {
                // Keep original column names, including special characters
                dt.Columns.Add(header, typeof(string));
            }
            return dt;
        }

        private static void ProcessExcelRow(IXLRow row, DataTable dt, int expectedColumnCount)
        {
            DataRow dataRow = dt.NewRow();

            // Process each cell in the row
            for (int i = 0; i < expectedColumnCount; i++)
            {
                var cell = row.Cell(i + 1);
                dataRow[i] = GetCellValue(cell);
            }

            dt.Rows.Add(dataRow);
        }

        private static object GetCellValue(IXLCell cell)
        {
            if (cell.IsEmpty())
                return DBNull.Value;

            // Convert Excel values to appropriate string representations
            return cell.DataType switch
            {
                XLDataType.DateTime => cell.GetDateTime().ToString("yyyy-MM-dd HH:mm:ss"),
                XLDataType.Number => cell.GetDouble().ToString(System.Globalization.CultureInfo.InvariantCulture),
                XLDataType.Boolean => cell.GetBoolean().ToString(),
                _ => cell.GetString().Trim()
            };
        }
    }
}