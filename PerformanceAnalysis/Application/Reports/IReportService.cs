using PerformanceAnalysis.Reports.DayOfWeekActivity;
using PerformanceAnalysis.Reports.GroupLeadersAndLaggards;
using PerformanceAnalysis.Reports.GroupTrend;
using PerformanceAnalysis.Reports.StudentMonthlyProgress;
using PerformanceAnalysis.Reports.StudentPassRate;
using PerformanceAnalysis.Reports.StudentPassRateSummary;
using PerformanceAnalysis.Reports.StudentRating;
using PerformanceAnalysis.Reports.StudentTestResults;

namespace PerformanceAnalysis.Application.Reports
{
    public interface IReportService
    {
        Task<IEnumerable<GroupLeadersAndLaggardsItem>> GetGroupLeadersAndLaggardsAsync(GroupLeadersAndLaggardsFilter filter);
        Task<IEnumerable<StudentTestResultsItem>> GetStudentTestResultsAsync(StudentTestResultsFilter filter);
        Task<IEnumerable<GroupTrendItem>> GetGroupTrendAsync(GroupTrendFilter filter);
        Task<IEnumerable<StudentRatingItem>> GetStudentRatingAsync(StudentRatingFilter filter);
        Task<IEnumerable<StudentMonthlyProgressItem>>
 GetStudentMonthlyProgressAsync(StudentMonthlyProgressFilter filter);
        Task<IEnumerable<StudentPassRateItem>> GetStudentPassRateAsync(StudentPassRateFilter filter);
        Task<StudentPassRateSummaryItem?> GetStudentPassRateSummaryAsync(StudentPassRateSummaryFilter filter);
        Task<IEnumerable<DayOfWeekActivityItem>> GetDayOfWeekActivityAsync(DayOfWeekActivityFilter filter);

    }
}
