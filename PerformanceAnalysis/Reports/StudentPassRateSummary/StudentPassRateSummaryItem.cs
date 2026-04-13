namespace PerformanceAnalysis.Reports.StudentPassRateSummary
{
    public class StudentPassRateSummaryItem
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int TestsAttempted { get; set; }
        public int TestsPassed { get; set; }
        public decimal PassRate { get; set; }
        public int TotalScore { get; set; }
        public decimal AverageScore { get; set; }
    }
}
