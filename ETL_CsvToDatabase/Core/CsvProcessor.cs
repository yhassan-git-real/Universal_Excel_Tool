using CsvHelper;
using CsvHelper.Configuration;
using System.Data;
using System.Globalization;
using System.Text;
using Microsoft.Data.SqlClient;
using ETL_CsvToDatabase.Models;
using ETL_CsvToDatabase.Services;

namespace ETL_CsvToDatabase.Core
{
    public static class CsvProcessor
    {
        public static IEnumerable<BatchResult> LoadCsvInBatches(
            string csvFilePath,
            SqlConnection connection,
            string errorTableName,
            int batchSize,
            bool enableProgressNotifications = true,
            int progressNotificationInterval = 50000)
        {
            // Validate CSV structure before processing
            ValidateCsvStructure(csvFilePath);

            // Pre-count total rows for accurate progress reporting
            long totalRows = CountCsvRows(csvFilePath);
            ConsoleLogger.LogInfo("file", $"Total rows to process: {totalRows:N0}");

            // Configure CsvHelper
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                BadDataFound = context =>
                {
                    // Log bad data and continue
                    _ = LoggingService.LogError(
                        connection,
                        errorTableName,
                        Path.GetFileName(csvFilePath),
                        $"Row {context.Context.Parser.Row}",
                        "BadData",
                        $"Bad data found: {context.RawRecord}");
                    ConsoleLogger.LogError($"Bad data in row {context.Context.Parser.Row}: {context.RawRecord}");
                },
                MissingFieldFound = null, // Allow missing fields (treat as NULL)
                HeaderValidated = null,   // Skip header validation
                Encoding = Encoding.UTF8
            };

            using var reader = new StreamReader(csvFilePath, Encoding.UTF8);
            using var csv = new CsvReader(reader, config);

            // Read header row
            csv.Read();
            csv.ReadHeader();
            string[]? headers = csv.HeaderRecord;

            if (headers == null || headers.Length == 0)
            {
                throw new InvalidOperationException($"CSV file has no header row: {csvFilePath}");
            }

            ConsoleLogger.LogInfo("file", $"Processing CSV with {headers.Length} columns");

            var currentBatch = CreateDataTable(headers);
            bool isFirstBatch = true;
            int processedRows = 0;
            int expectedColumnCount = headers.Length;

            // Process data rows
            while (csv.Read())
            {
                bool rowProcessedSuccessfully = true;
                try
                {
                    // Validate column count consistency
                    int actualColumnCount = csv.Parser.Count;
                    if (actualColumnCount != expectedColumnCount)
                    {
                        throw new InvalidOperationException(
                            $"Column count mismatch at row {csv.Parser.Row}. Expected {expectedColumnCount}, found {actualColumnCount}");
                    }

                    ProcessCsvRow(csv, currentBatch, headers.Length);
                    processedRows++;

                    if (enableProgressNotifications && processedRows % progressNotificationInterval == 0)
                    {
                        ConsoleLogger.LogProgress("Processing CSV rows", processedRows, totalRows);
                    }
                }
                catch (Exception ex)
                {
                    rowProcessedSuccessfully = false;
                    // Log error and continue processing
                    _ = LoggingService.LogError(
                        connection,
                        errorTableName,
                        Path.GetFileName(csvFilePath),
                        $"Row {csv.Parser.Row}",
                        "Data Processing Error",
                        ex.Message);

                    ConsoleLogger.LogError($"Error processing row {csv.Parser.Row}: {ex.Message}");
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

            // ALWAYS return the final batch - even if empty after yielding a full batch
            // This ensures IsLastBatch is always set to true for the final batch
            yield return new BatchResult
            {
                Data = currentBatch,
                IsFirstBatch = isFirstBatch,
                IsLastBatch = true
            };

            if (processedRows == 0)
            {
                // Empty CSV file (header only)
                ConsoleLogger.LogWarning($"CSV file is empty (no data rows): {Path.GetFileName(csvFilePath)}");
            }

            ConsoleLogger.LogSuccess($"Completed processing {processedRows:N0} rows");
        }

        private static long CountCsvRows(string csvFilePath)
        {
            long count = 0;
            try
            {
                using var reader = new StreamReader(csvFilePath, Encoding.UTF8);
                // Skip header
                reader.ReadLine();
                // Count data rows
                while (!reader.EndOfStream)
                {
                    reader.ReadLine();
                    count++;
                }
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogWarning($"Could not pre-count rows: {ex.Message}. Progress will show incremental count.");
                return 0; // Return 0 to skip progress percentage calculation
            }
            return count;
        }

        public static void ValidateCsvStructure(string csvFilePath)
        {
            if (!File.Exists(csvFilePath))
            {
                throw new FileNotFoundException($"CSV file not found: {csvFilePath}");
            }

            try
            {
                // Try to open and read the file
                using var reader = new StreamReader(csvFilePath, Encoding.UTF8);
                string? firstLine = reader.ReadLine();

                if (string.IsNullOrWhiteSpace(firstLine))
                {
                    throw new InvalidOperationException($"CSV file is empty or has no header row: {csvFilePath}");
                }

                ConsoleLogger.LogInfo("validation", $"CSV structure validated: {Path.GetFileName(csvFilePath)}");
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException($"Cannot read CSV file (file may be corrupted or in use): {csvFilePath}", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new InvalidOperationException($"Access denied to CSV file: {csvFilePath}", ex);
            }
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

        private static void ProcessCsvRow(CsvReader csv, DataTable dt, int expectedColumnCount)
        {
            DataRow dataRow = dt.NewRow();

            // Process each cell in the row
            for (int i = 0; i < expectedColumnCount; i++)
            {
                string? value = csv.GetField(i);
                dataRow[i] = string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();
            }

            dt.Rows.Add(dataRow);
        }
    }
}
