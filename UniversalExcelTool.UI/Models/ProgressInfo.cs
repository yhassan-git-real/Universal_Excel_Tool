using System;

namespace UniversalExcelTool.UI.Models
{
    /// <summary>
    /// Represents progress information for ETL operations
    /// </summary>
    public class ProgressInfo
    {
        public double OverallProgress { get; set; }
        public int CurrentFile { get; set; }
        public int TotalFiles { get; set; }
        public string? CurrentFileName { get; set; }
        public long CurrentRow { get; set; }
        public long TotalRows { get; set; }
        public string Status { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan Elapsed { get; set; }
        public TimeSpan? EstimatedTimeRemaining { get; set; }
        public bool IsComplete { get; set; }
        public bool IsError { get; set; }
        public string? ErrorMessage { get; set; }

        public ProgressInfo()
        {
            Status = "Ready";
            StartTime = DateTime.Now;
        }

        /// <summary>
        /// Gets progress percentage as string
        /// </summary>
        public string ProgressPercentage => $"{OverallProgress:F1}%";

        /// <summary>
        /// Gets file progress as string
        /// </summary>
        public string FileProgress => TotalFiles > 0 
            ? $"File {CurrentFile} of {TotalFiles}" 
            : "No files";

        /// <summary>
        /// Gets row progress as string
        /// </summary>
        public string RowProgress => TotalRows > 0 
            ? $"{CurrentRow:N0} / {TotalRows:N0} rows" 
            : "Processing...";

        /// <summary>
        /// Gets elapsed time as formatted string
        /// </summary>
        public string ElapsedTime => Elapsed.ToString(@"hh\:mm\:ss");

        /// <summary>
        /// Gets estimated time remaining as formatted string
        /// </summary>
        public string EstimatedTime => EstimatedTimeRemaining.HasValue 
            ? EstimatedTimeRemaining.Value.ToString(@"hh\:mm\:ss") 
            : "Calculating...";

        /// <summary>
        /// Updates estimated time remaining based on progress
        /// </summary>
        public void UpdateEstimatedTime()
        {
            if (OverallProgress > 0 && OverallProgress < 100)
            {
                var totalEstimatedTime = TimeSpan.FromSeconds(
                    Elapsed.TotalSeconds / (OverallProgress / 100.0));
                EstimatedTimeRemaining = totalEstimatedTime - Elapsed;
            }
            else
            {
                EstimatedTimeRemaining = null;
            }
        }
    }
}
