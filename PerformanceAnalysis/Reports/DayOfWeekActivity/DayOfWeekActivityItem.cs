namespace PerformanceAnalysis.Reports.DayOfWeekActivity
{
    public class DayOfWeekActivityItem
    {
        public int DayOfWeek { get; set; }
        public string DayName { get; set; } = string.Empty;
        public int TestsCompleted { get; set; }
        public int UniqueStudents { get; set; }
    }
}
