namespace PerformanceAnalysis.Reports.GroupTrend
{
    public class GroupTrendItem
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public DateTime? Month { get; set; }
        public string MonthLabel { get; set; } = string.Empty;
        public decimal AverageScore { get; set; }
        public int AttemptsCount { get; set; }
    }
}
