using ETL_Excel.Models;
using ClosedXML.Excel;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ETL_Excel.Modules
{
    public static class ExcelProcessor
    {
        private static ConfigurationModel _config = ConfigurationManager.GetConfig();
        private static int specialSheetsMoved = 0; // Add this counter

        public static async Task<(int totalSheets, int processedSheets, int errors, int specialSheetsMoved)> ProcessExcelFileAsync(string filePath, int totalFiles, int currentFileIndex, ConsoleLogger consoleLogger)
        {
            int totalSheets = 0;
            int processedSheets = 0;
            int errors = 0;

            try
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    totalSheets = workbook.Worksheets.Count;

                    for (int i = 1; i <= totalSheets; i++)
                    {
                        var worksheet = workbook.Worksheet(i);
                        string fileInfo = ConsoleHelper.GetFileInfo(Path.GetFileName(filePath), i, totalSheets);
                        consoleLogger.CaptureConsoleOutput(fileInfo, true);

                        var isProcessed = await SplitSheetAsync(filePath, worksheet, consoleLogger);

                        if (isProcessed)
                        {
                            processedSheets++;
                        }
                        else
                        {
                            errors++;
                        }

                        if (i % 10 == 0 || i == totalSheets) // Update progress less frequently
                        {
                            string progress = ConsoleHelper.GetProgress(currentFileIndex, totalFiles, i, totalSheets, (double)i / totalSheets);
                            consoleLogger.CaptureConsoleOutput(progress);
                        }
                    }
                }
                LogManager.LogSuccess($"Processed file: {filePath}");
            }
            catch (Exception ex)
            {
                LogManager.LogError($"Error processing file {filePath}: {ex.Message}");
                errors++;
            }

            return (totalSheets, processedSheets, errors, specialSheetsMoved);
        }

        private static async Task<bool> SplitSheetAsync(string sourceFilePath, IXLWorksheet sourceWorksheet, ConsoleLogger consoleLogger)
        {
            try
            {
                string outputFolder = DetermineOutputFolder(sourceWorksheet.Name, _config);
                string outputFilePath = Path.Combine(outputFolder,
                    $"{Path.GetFileNameWithoutExtension(sourceFilePath)}_{sourceWorksheet.Name}_{sourceWorksheet.Position:D2}.xlsx");

                outputFilePath = FileManager.PrepareOutputFile(outputFilePath, consoleLogger);

                using (var destWorkbook = new XLWorkbook())
                {
                    var destWorksheet = destWorkbook.Worksheets.Add(sourceWorksheet.Name);
                    CopySheetData(sourceWorksheet, destWorksheet);

                    // Configure workbook settings
                    destWorkbook.CalculateMode = XLCalculateMode.Manual;
                    await Task.Run(() => destWorkbook.SaveAs(outputFilePath));
                }

                if (!File.Exists(outputFilePath))
                {
                    throw new Exception($"Failed to create output file: {outputFilePath}");
                }

                string sheetInfo = ConsoleHelper.GetSheetInfo(sourceWorksheet.Name, sourceWorksheet.RowsUsed().Count(),
                    sourceWorksheet.ColumnsUsed().Count(), Path.GetFileName(outputFilePath));
                consoleLogger.CaptureConsoleOutput(sheetInfo, true);
                LogManager.LogSuccess($"Created file: {outputFilePath}");

                // Increment the special sheets moved counter if the sheet is special
                if (outputFolder.Contains(_config.OutputSettings.OtherCategoryFolder))
                {
                    specialSheetsMoved++;
                }

                return true;
            }
            catch (Exception ex)
            {
                LogManager.LogError($"Error processing sheet {sourceWorksheet.Name}: {ex.Message}");
                return false;
            }
        }

        private static void CopySheetData(IXLWorksheet source, IXLWorksheet destination)
        {
            try
            {
                LogManager.LogSuccess($"Starting to copy sheet: {source.Name}");

                var usedRange = source.RangeUsed();
                if (usedRange == null) return;

                // First, copy all values and styles to maintain data integrity
                foreach (var row in usedRange.Rows())
                {
                    foreach (var cell in row.Cells())
                    {
                        var destCell = destination.Cell(cell.Address);

                        // Check if the cell starts with a formula indicator
                        var cellValue = cell.GetString().Trim();
                        if (cellValue.StartsWith("="))
                        {
                            try
                            {
                                // Only try to copy as formula if it actually starts with '='
                                destCell.FormulaA1 = cell.FormulaA1;
                            }
                            catch
                            {
                                // If formula fails, just copy the value as text
                                destCell.Value = cellValue;
                            }
                        }
                        else
                        {
                            // For non-formula cells, copy the value directly
                            destCell.Value = cell.Value;
                        }

                        // Copy the cell style
                        destCell.Style = cell.Style;
                    }
                }

                // Copy column widths to maintain sheet formatting
                foreach (var column in source.ColumnsUsed())
                {
                    destination.Column(column.ColumnNumber()).Width = column.Width;
                }

                // Copy essential worksheet settings
                destination.SheetView.ZoomScale = source.SheetView.ZoomScale;
                destination.PageSetup.PaperSize = source.PageSetup.PaperSize;
                destination.PageSetup.PageOrientation = source.PageSetup.PageOrientation;

                // Copy print settings
                try
                {
                    destination.PageSetup.Scale = source.PageSetup.Scale;
                    destination.PageSetup.PagesWide = source.PageSetup.PagesWide;
                    destination.PageSetup.PagesTall = source.PageSetup.PagesTall;
                }
                catch (Exception ex)
                {
                    LogManager.LogError($"Warning: Could not copy print settings: {ex.Message}");
                }

                LogManager.LogSuccess($"Finished copying sheet: {source.Name}");
            }
            catch (Exception ex)
            {
                LogManager.LogError($"Error in CopySheetData: {ex.Message}");
                throw;
            }
        }

        private static string DetermineOutputFolder(string sheetName, ConfigurationModel config)
        {
            // Check for special keywords first
            bool isSpecialSheet = config.OutputSettings.SpecialSheetKeywords
                .Any(keyword => sheetName.Contains(keyword, StringComparison.OrdinalIgnoreCase));

            if (isSpecialSheet)
            {
                return Path.Combine(
                    config.OutputSettings.BaseFolderPath,
                    config.OutputSettings.OtherCategoryFolder
                );
            }

            return config.OutputSettings.BaseFolderPath;
        }
    }
}

