namespace PerformanceAnalysis.Reports.StudentRating
{
    public class StudentRatingItem
    {
        public int Rank { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public int TotalScore { get; set; }
    }
}
